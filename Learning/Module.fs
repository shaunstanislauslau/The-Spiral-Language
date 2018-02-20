﻿module Learning.Module

open Spiral.Types
open Spiral.Lib

let mnist =
    (
    "Mnist",[extern_;host_tensor],"The Mnist loader module.",
    """
open Extern
met load_mnist kind (!dyn filename) =
    inl enum ty x = FS.StaticField ty x ty
    inl File_ty = fs [text: "System.IO.File"]
    inl FileStream_ty = fs [text: "System.IO.FileStream"]
    inl FileMode = enum (fs [text: "System.IO.FileMode"])
    inl FileAccess = enum (fs [text: "System.IO.FileAccess"])
    inl FileShare = enum (fs [text: "System.IO.FileShare"])
    inl BinaryReader_ty = fs [text: "System.IO.BinaryReader"]
    inl IPAddress_ty = fs [text: "System.Net.IPAddress"]

    inl netword_to_host_order x = FS.StaticMethod IPAddress_ty .NetworkToHostOrder x int32

    use f = FS.StaticMethod File_ty .Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read) FileStream_ty
    use d = FS.Constructor BinaryReader_ty f

    inl read_int32 x = FS.Method d .ReadInt32 x int32 |> netword_to_host_order
    inl read_bytes n = FS.Method d .ReadBytes n (array uint8)

    inl to_ints64 = Tuple.map (to int64)

    inl magic_number = read_int32()
    match kind with
    | .label ->
        assert (magic_number = 2049i32) "Expected a 2049i32 magic number."
        inl n = read_int32()
        to int64 n, read_bytes n
    | .image ->
        assert (magic_number = 2051i32) "Expected a 2051i32 magic number."
        inl n, rows, cols = read_int32(), read_int32(), read_int32()
        to_ints64 (n, rows, cols), read_bytes (n * rows * cols)


inl load_mnist_tensors mnist_path =
    inl mnist_files = {
        test_images = {file = "t10k-images.idx3-ubyte"; image_size = 10000,28,28}
        test_labels = {file = "t10k-labels.idx1-ubyte"; label_size = 10000}
        train_images = {file = "train-images.idx3-ubyte"; image_size = 60000,28,28}
        train_labels = {file = "train-labels.idx1-ubyte"; label_size = 60000}
        }
           
    inl path_type = fs [text: "System.IO.Path"]
    inl combine x = FS.StaticMethod path_type .Combine x string
    
    module_map (inl _ -> function
        | {file image_size} ->
            inl size, ar = load_mnist .image (combine (mnist_path, file))
            assert (image_size = size) "Mnist dimensions do not match the expected values."
            inl images, rows, cols = image_size
            HostTensor.array_as_tensor ar
            |> HostTensor.split (const (images, rows * cols))
            |> HostTensor.map (inl x -> to float32 x / 255f32)
            
        | {file label_size} ->
            inl n, ar = load_mnist .label (combine (mnist_path, file))
            assert (label_size = n) "Mnist dimensions do not match the expected values."
            HostTensor.init (label_size, 10) (inl a ->
                inl x = ar a
                inl b -> if to uint8 b = x then 1.0f32 else 0.0f32
                )
        ) mnist_files

{load_mnist_tensors}
    """) |> module_

let resize_array =
    (
    "ResizeArray",[extern_],"The resizable array module.",
    """
open Extern
inl create {d with elem_type} =
    inl ty = fs [text: "ResizeArray"; types: elem_type]
    inl x =
        match d with
        | {size} -> FS.Constructor ty (to int32 size)
        | _ -> FS.Constructor ty ()

    inl filter f = FS.Method x ."RemoveAll <| System.Predicate" (closure_of f (elem_type => bool)) int32 |> ignore
    inl sort f =
        inl comparison_type = fs [text: "System.Comparison"; types: elem_type]
        inl f = closure_of f (elem_type => elem_type => int32)
        inl c = FS.Constructor comparison_type f
        FS.Method x .Sort c ()

    inl index i = macro.fs elem_type [arg: x; text: ".["; arg: to int32 i; text: "]"]
    inl set i v = macro.fs () [arg: x; text: ".["; arg: to int32 i; text: "] <- "; arg: v]
    inl clear () = FS.Method x .Clear() ()
    inl count () = FS.Method x .get_Count() int32
    inl add y = FS.Method x .Add y ()

    inl iter f = FS.Method x ."ForEach <| System.Action<_>" (closure_of f (elem_type => ())) ()

    function
    | .sort -> sort 
    | .filter -> filter 
    | .set -> set
    | .clear -> clear ()
    | .count -> count ()
    | .add -> add
    | .iter -> iter
    | .elem_type -> elem_type
    | i -> index i
{ 
create
} |> stack
    """
    ) |> module_

let allocator = 
    (
    "Allocator",[resize_array;loops;option;extern_;console],"The section based GPU memory allocator module.",
    """
open Extern

inl SizeT_type = fs [text: "ManagedCuda.BasicTypes.SizeT"]
inl CUdeviceptr_type = fs [text: "ManagedCuda.BasicTypes.CUdeviceptr"]
inl SizeT = FS.Constructor SizeT_type
inl CUdeviceptr = FS.Constructor CUdeviceptr_type << SizeT

inl smartptr_create (ptr: uint64) =
    inl cell = ref ptr
    function
    | .Dispose -> cell := 0u64
    | .Try -> cell()
    | () -> join 
        inl x = cell ()
        assert (x <> 0u64) "A Cuda memory cell that has been disposed has been tried to be accessed."
        x
    |> stack

inl mult = 256u64
assert (mult <> 0u64 && (mult &&& mult - 1u64) = 0u64) "Multiple must be a power of 2." 
inl round_up_to_multiple size = (size + mult - 1u64) &&& 0u64 - mult

inl to_uint x = FS.UnOp .uint64 x uint64
inl allocate_global s =
    to uint64 >> round_up_to_multiple >> dyn
    >> inl size -> { size ptr = FS.Method s.context .AllocateMemory (SizeT size) CUdeviceptr_type |> to_uint |> smartptr_create }

inl compare a b = if a < b then -1i32 elif a = b then 0i32 else 1i32
inl sort_ptrs x = x.sort (inl {ptr=a} {ptr=b} -> compare (a()) (b()))
inl sort_sizes x = x.sort (inl {size=a} {size=b} -> compare b a)

met free_cells_refresh {section with pool free_cells used_cells} = 
    used_cells.filter (inl {ptr} -> ptr.Try = 0u64)
    sort_ptrs used_cells
    free_cells.clear
    inl add {ptr size} = 
        inl ptr' = round_up_to_multiple ptr
        inl d = ptr' - ptr
        if size > d then free_cells.add {ptr = ptr'; size=size-d}

    inl start x = x.ptr()
    inl end x = x.ptr() + x.size

    Loops.for {from=0i32; near_to=used_cells.count; by=1i32; state=start pool; body=inl {state=ptr i} ->
        inl state' = used_cells i
        assert (start state' >= ptr) "The next pointer should be higher than the last."
        
        inl start = start state'
        add { ptr size = start - ptr }
        start + state'.size // end state'
        }
    |> inl ptr -> // This is for the final free cell at the end.
        inl size = end pool - ptr
        add {ptr size}


met allocate {section with pool used_cells free_cells} (!(to uint64 >> round_up_to_multiple >> dyn) size') =
    inl loop next =
        inl {ptr size} = free_cells 0i32
        if size' <= size then
            free_cells.set 0i32 {ptr=ptr+size'; size=size-size'}
            {ptr=smartptr_create ptr; size=size'}
        else next()

    inl x =
        assert (free_cells.count > 0i32) "Out of memory in the designated section."
        loop <| inl _ ->
            sort_sizes free_cells
            loop <| inl _ -> 
                free_cells_refresh section
                sort_sizes free_cells
                loop <| inl _ ->
                    failwith pool "Out of memory in the designated section."
    used_cells.add x
    x.ptr

inl section_create s size ret =
    inl pool = allocate_global s size
    inl free_cells = ResizeArray.create {elem_type=type {ptr=uint64; size=uint64}}
    inl used_cells = ResizeArray.create {elem_type=type pool}
    inl section = {pool free_cells used_cells}
    free_cells_refresh section

    inl allocate _ = 
        function
        | .elem_type -> type pool.ptr
        | .refresh -> free_cells_refresh section
        | x -> allocate section x
        |> heap

    inl r = s.module_add .Section {allocate} |> ret

    inl ptr = pool.ptr
    FS.Method s.context .FreeMemory (ptr() |> CUdeviceptr) unit
    ptr.Dispose
    r

stack section_create
    """) |> module_

let region =
    (
    "Region",[resize_array],"The region based resource tracker.",
    """
inl counter_ref_create ptr =
    inl count = ref 0
    function
    | .inc -> count := count() + 1
    | .dec -> 
        count := count() - 1
        if count() = 0 then ptr.Dispose
    | x -> ptr x
    |> heap

inl assign {region_name} s r = 
    inl region = s region_name
    join r.inc; region.add r

inl allocate d s (!dyn x) =
    inl allocate = d.allocate s
    join
        inl r = allocate x |> counter_ref_create
        assign d s r; r

inl allocate_mem d s (!dyn x) =
    inl allocate = d.allocate s
    join
        inl r = allocate x |> counter_ref_create
        assign d s r; r

inl allocate_stream d s =
    inl allocate = d.allocate s
    join
        inl r = counter_ref_create allocate
        assign d s r; r
    |> (const >> s.member_add .stream)

inl clear {region_name} s =
    inl region = s region_name
    join 
        region.iter (inl r -> r.dec)
        region.clear

inl create {region_name allocate} s = 
    inl elem_type = type counter_ref_create (var (allocate s).elem_type)
    inl region = ResizeArray.create {elem_type}
    s.member_add region_name (const region)

inl create' {region_module_name} s ret =
    inl s = s region_module_name .create
    inl r = ret s
    s region_module_name .clear
    r

inl methods_template d = 
    {
    assign = assign d
    clear = clear d
    create = create d 
    create' = create' d
    } |> stack

inl methods_mem = 
    inl region_module_name = .RegionMem
    inl d = {
        region_module_name
        region_name = .region_mem
        allocate = inl s -> s.Section.allocate
        }
    inl x = methods_template d
    region_module_name, {x with allocate = allocate_mem d}

inl methods_stream =
    inl region_module_name = .RegionStream
    inl d = {
        region_module_name
        region_name = .region_stream
        allocate = inl s -> s.Stream.allocate
        }
    inl x = methods_template d
    region_module_name, {x with
        allocate = allocate_stream d 
        create = inl s -> (self s).RegionStream.allocate
        }

inl s -> 
    inl add s (a, b) = s.module_add a b
    Tuple.foldl add s (methods_mem, methods_stream)
    """) |> module_

let cuda_stream = 
    (
    "CudaStream",[extern_],"The Cuda stream module.",
    """
open Extern
inl ty x = fs [text: x]
inl CudaStream_type = ty "ManagedCuda.CudaStream"
inl CUstream_type = ty "ManagedCuda.BasicTypes.CUstream"
inl dispose x = FS.Method x .Dispose () ()

inl rec allocate _ =
    inl is_live = ref true
    inl stream = FS.Constructor CudaStream_type ()
    function
    | .Dispose -> 
        dispose stream
        is_live := false
    | .elem_type -> type allocate ()
    | x -> join
        assert (is_live()) "The stream has been disposed."
        match x with
        | .extract -> macro.fs CUstream_type [arg: stream; text: ".Stream"]
        | .synchronize -> FS.Method stream .Synchronize() ()
        | .wait_on on ->
            inl event_type = fs [text: "ManagedCuda.CudaEvent"]
            inl event = FS.Constructor event_type ()
            FS.Method event .Record on.extract ()
            macro.fs () [arg: stream; text: ".WaitEvent "; arg: event; text: ".Event"]
            dispose event
        | () -> stream
    |> heap

inl s -> s.module_add .Stream {allocate}
    """) |> module_

let cuda_tensor = 
    (
    "CudaTensor",[extern_;host_tensor],"The Cuda tensor module.",
    """
open HostTensor
open Extern

inl SizeT_type = fs [text: "ManagedCuda.BasicTypes.SizeT"]
inl CUdeviceptr_type = fs [text: "ManagedCuda.BasicTypes.CUdeviceptr"]
inl SizeT = FS.Constructor SizeT_type
inl CUdeviceptr = FS.Constructor CUdeviceptr_type << SizeT

inl ptr_cuda {ar offset} ret = ar.ptr() + to uint64 (offset * sizeof ar.elem_type) |> ret
inl CUResult_ty = fs [text: "ManagedCuda.BasicTypes.CUResult"]
inl assert_curesult res = macro.fs unit [text: "if "; arg: res; text: " <> ManagedCuda.BasicTypes.CUResult.Success then raise <| new ManagedCuda.CudaException"; args: res]
inl memcpy dst_ptr src_ptr size = macro.fs CUResult_ty [text: "ManagedCuda.DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpy"; args: CUdeviceptr dst_ptr, CUdeviceptr src_ptr, SizeT size] |> assert_curesult

inl GCHandle_ty = fs [text: "System.Runtime.InteropServices.GCHandle"]
inl ptr_dotnet {ar offset} ret =
    inl elem_type = ar.elem_type
    inl handle = macro.fs GCHandle_ty [type:GCHandle_ty; text: ".Alloc"; parenth: [arg: ar; text: "System.Runtime.InteropServices.GCHandleType.Pinned"]]
    inl r =
        macro.fs int64 [arg: handle; text: ".AddrOfPinnedObject().ToInt64()"] 
        |> to uint64 |> (+) (to uint64 (offset * sizeof elem_type)) |> ret
    macro.fs unit [arg: handle; text: ".Free()"]
    r

inl copy span dst {src with ar size ptr_get} =
    inl elem_type = ar.elem_type 
    assert (blittable_is elem_type) "The host array type must be blittable."
    inl span_size = match size with () -> span | size :: _ -> span * size
    inb src = ptr_get src
    inl memcpy dst = memcpy dst src (span_size * sizeof elem_type)
    match dst with
    | {ar size=size' ptr_get} -> 
        assert (size' = size) "The source and the destination must have the same sizes."
        assert (eq_type ar.elem_type elem_type) "The source and the destination must have the same types"
        inb dst = ptr_get dst
        memcpy dst
    | {array_create ptr_get} -> 
        inl ar = array_create elem_type span_size
        inb dst = ptr_get {ar offset=0}
        memcpy dst
        ar

inl transfer_template f tns = 
    assert_contiguous tns
    inl f = f tns.span_outer
    tns.update_body <| inl body -> {body with ar = f body}

inl get_elem s {src with size=()} = s.CudaTensor.to_host_array 1 src 0
met set_elem s (!dyn {dst with size=()}) (!dyn v) =
    inl ar = array_create v 1
    ar 0 <- v
    copy 1 {dst with ptr_get=ptr_cuda} {ar size=(); offset=0; ptr_get=ptr_dotnet}

inl to_dev_tensor data = HostTensor.tensor_methods.update_body data (inl body -> 
    inb ptr = ptr_cuda body
    {body with ar=!UnsafeCoerceToArrayCudaGlobal(ptr,body.ar.elem_type); offset=0}
    )

inl methods = // The reason it is done like this is so the methods do not get renamed every time they are passed through a join point.
    inl x = HostTensor.tensor_methods
    {x with to_dev_tensor} |> stack

inl array_create_cuda_global s elem_type len =
    inl ptr = s.RegionMem.allocate (len * sizeof elem_type)
    function // It needs to be like this rather than a module so toa_map does not split it.
    | .elem_type -> elem_type
    | .ptr -> ptr
    |> stack

inl clear' s x = s.CudaTensor.clear x; x

inl methods = 
    {
    create=inl s data -> create {data with array_create = array_create_cuda_global s} .update_methods methods
    create_like=inl s tns -> s.CudaTensor.create {elem_type=tns.elem_type; dim=tns.dim}

    from_host_array=met s (!dyn span) (!dyn {src with ar offset size}) ->
        copy span {array_create=array_create_cuda_global s; ptr_get=ptr_cuda} {src with ptr_get=ptr_dotnet}

    to_host_array=met s (!dyn span) (!dyn {src with ar offset size}) ->
        copy span {array_create ptr_get=ptr_dotnet} {src with ptr_get=ptr_cuda}

    get=inl s tns ->
        match tns.unwrap with
        | {bodies dim=()} -> toa_map s.CudaTensor.get_elem bodies
        | _ -> error_type "Cannot get from tensor whose dimensions have not been applied completely."

    set=inl s tns v ->
        match tns.unwrap with
        | {bodies dim=()} -> toa_iter2 s.CudaTensor.set_elem bodies v
        | _ -> error_type "Cannot set to a tensor whose dimensions have not been applied completely."

    from_host_tensor=inl s x -> transfer_template s.CudaTensor.from_host_array x .update_methods methods
    from_host_tensors=inl s -> toa_map s.CudaTensor.from_host_tensor
    to_host_tensor=inl s x -> transfer_template s.CudaTensor.to_host_array x .update_methods HostTensor.tensor_methods

    clear=inl s tns ->
        assert_contiguous tns
        inl span = tns.span_outer
        inl stream = s.stream.extract
        tns.update_body <| inl {body with size ar} ->
            inl size = match size with () -> 1 | x :: _ -> x
            FS.Method s.context .ClearMemoryAsync (CUdeviceptr (ar.ptr()), 0u8, size * span * sizeof ar.elem_type |> SizeT, stream) unit
        |> ignore
    
    zero=inl s -> s.CudaTensor.create >> clear' s
    zero_like=inl s -> s.CudaTensor.create_like >> clear' s

    print=met s (!dyn x) -> s.CudaTensor.to_host_tensor x |> HostTensor.print
    } |> stack

inl s -> s.module_add .CudaTensor methods
    """) |> module_

let cuda_random =
    (
    "CudaRandom",[extern_;cuda_tensor],"The CudaRandom module.",
    """
inl s ret ->
    open Extern

    inl SizeT_type = fs [text: "ManagedCuda.BasicTypes.SizeT"]
    inl CUdeviceptr_type = fs [text: "ManagedCuda.BasicTypes.CUdeviceptr"]
    inl SizeT = FS.Constructor SizeT_type
    inl CUdeviceptr = FS.Constructor CUdeviceptr_type << SizeT
    
    use random = 
        inl generator_type = fs [text: "ManagedCuda.CudaRand.GeneratorType"]
        FS.Constructor (fs [text: "ManagedCuda.CudaRand.CudaRandDevice"]) (FS.StaticField generator_type .PseudoDefault generator_type)
    
    open HostTensor
    
    inl fill_array s distribution (!SizeT size1d) ar =
        FS.Method random .SetStream s.stream.extract unit
        inl elem_type = ar.elem_type
        inl ar = CUdeviceptr ar
        inl gen, dot = "Generate", "."
        match distribution with
        | .Uniform ->
            inl args = ar, size1d
            inl bits = 
                match elem_type with
                | _ : float32 -> "32" | _ : float64 -> "64"
                | _ -> error_type ("Only 32/64 bit float types are supported. Try UInt if you need uint random numbers. Got: ", elem_type)
            macro.fs unit [arg: random; text: dot; text: gen; text: distribution; text: bits; args: args]
        | {dst=(.Normal | .LogNormal) & distribution stddev mean} ->
            match stddev with | _: float32 -> () | _ -> error_type "Standard deviation needs to be in float32."
            match mean with | _: float32 -> () | _ -> error_type "Mean needs to be in float32."

            inl args = ar, size1d, mean, stddev
            inl bits = 
                match elem_type with
                | _ : float32 -> "32" | _ : float64 -> "64"
                | _ -> error_type ("Only 32/64 bit float types are supported. Try UInt if you need uint random numbers. Got: ", elem_type)
            macro.fs unit [arg: random; text: dot; text: gen; text: distribution; text: bits; args: args]
        | .UInt -> // every bit random
            inl args = ar, size1d
            inl bits =
                match elem_type with
                | _ : uint32 -> "32" | _ : uint64 -> "64"
                | _ -> error_type "Only 32/64 bit uint types are supported."
            macro.fs unit [arg: random; text: dot; text: gen; text: bits; args: args]

    inl fill s op (!zip in) =
        inl in' = flatten in .to_dev_tensor
        inl len = in'.length
        in'.update_body (inl {ar} -> fill_array s op len ar) |> ignore

    inl create s op dsc =
        inl device_tensor = s.CudaTensor.create dsc
        fill s op device_tensor
        device_tensor

    ret <| s.module_add .CudaRandom {fill create}
    """) |> module_

let cuda_blas =
    (
    "CudaBlas",[cuda_tensor;extern_],"The CudaBlas module.",
    """
inl s ret ->
    open Extern
    
    inl SizeT_type = fs [text: "ManagedCuda.BasicTypes.SizeT"]
    inl CUdeviceptr_type = fs [text: "ManagedCuda.BasicTypes.CUdeviceptr"]
    inl SizeT = FS.Constructor SizeT_type
    inl CUdeviceptr = FS.Constructor CUdeviceptr_type << SizeT

    inl enum ty x = FS.StaticField ty x ty

    inl operation_type = fs [text: "ManagedCuda.CudaBlas.Operation"]
    inl to_operation = function
        | .T -> enum operation_type .Transpose
        | .nT -> enum operation_type .NonTranspose

    inl isT = function
        | .T -> true
        | _ -> false

    inl isnT = function
        | .nT -> true
        | _ -> false

    inl len = HostTensor.span
    inl rows x = x.dim |> inl a,b -> len a
    inl cols x = x.dim |> inl a,b -> len b
    inl ld x = x.bodies.size |> fst

    use cublas =
        inl cublas_type = fs [text: "ManagedCuda.CudaBlas.CudaBlas"]
        inl pointer_mode_type = fs [text: "ManagedCuda.CudaBlas.PointerMode"]
        inl atomics_mode_type = fs [text: "ManagedCuda.CudaBlas.AtomicsMode"]
        FS.Constructor cublas_type (enum pointer_mode_type .Host, enum atomics_mode_type .Allowed)

    open HostTensor
    inl call s method args = 
        inl to_dev_tensor x = assert_contiguous x; x.to_dev_tensor
        inl args = Tuple.map (function x : int64 -> to int32 x | x -> x) args
        join 
            inl handle = FS.Method cublas .get_CublasHandle() (fs [text: "ManagedCuda.CudaBlas.CudaBlasHandle"])
            FS.Method cublas .set_Stream s.stream.extract ()
            inl args = 
                Tuple.map (function 
                    | x : float64 | x : float32 -> ref x
                    | (.nT | .T) as x -> to_operation x
                    | {ptr=!to_dev_tensor x} -> x.bodies.ar |> CUdeviceptr
                    | x -> x
                    ) args
            inl native_type = fs [text: "ManagedCuda.CudaBlas.CudaBlasNativeMethods"]
            inl status_type = fs [text: "ManagedCuda.CudaBlas.CublasStatus"]
            inl assert_ok status = macro.fs unit [text: "if "; arg: status; text: " <> ManagedCuda.CudaBlas.CublasStatus.Success then raise <| new ManagedCuda.CudaBlas.CudaBlasException"; args: status]
            FS.StaticMethod native_type method (handle :: args) status_type |> assert_ok

    /// General matrix-matrix multiply from cuBLAS. Inplace version
    inl gemm' s transa transb alpha A B beta C =
        inl a_col = if isnT transa then cols A else rows A
        inl b_row = if isnT transb then rows B else cols B
        assert (a_col = b_row) "Colums of a does not match rows of b in GEMM."

        inl m = if isnT transa then rows A else cols A
        inl n = if isnT transb then cols B else rows B
        inl k = a_col
        
        assert (m = rows C && n = cols C) "Output matrix dimensions do not match in GEMM."

        // The arguments are switched in order to convert from column major (which CuBlas uses) to row major (which Spiral's tensor use)
        call s .cublasSgemm_v2(transb, transa, n, m, k, alpha, {ptr=B}, ld B, {ptr=A}, ld A, beta, {ptr=C}, ld C)

    inl gemm s transa transb alpha A B =
        inl m = if isnT transa then rows A else cols A
        inl n = if isnT transb then cols B else rows B

        inl C = s.CudaTensor.create {dim=m,n; elem_type = A.elem_type}
        gemm' s transa transb alpha A B (to alpha 0) C
        C

    ret <| s.module_add .CudaBlas {gemm' gemm}
    """) |> module_

let cuda_kernel =
    (
    "CudaKernel",[lazy_;host_tensor;cuda_tensor],"The Cuda kernels module.",
    """
open HostTensor
open Extern

/// These two loops are only here until NVidia gets its shit together and fixes the NVCC tuple local write bugs for tail recursive loops.
inl whilecd {cond state body} =
    inl r = HostTensor.create {
        array_create=array_create_cuda_local 
        elem_type=state 
        dim=()
        }
    r .set state
    /// Note: While must have a join point around it.
    !While((join cond r.get), (r.set <| body r.get))
    r .get

inl forcd {d with from body} =
    inl finally =
        match d with
        | {finally} -> finally
        | _ -> id

    inl check =
        match d with
        | {near_to} from -> from < near_to 
        | {to} from -> from <= to
        | {down_to} from -> from >= down_to
        | {near_down_to} from -> from > near_down_to
        | _ -> error_type "Only one of `to`,`near_to`,`down_to`,`near_down_to` needs be present."

    inl by =
        match d with
        | {by} -> by
        | {to | near_to} -> 1
        | {down_to | near_down_to} -> -1

    inl to =
        match d with
        | {(to ^ near_to ^ down_to ^ near_down_to)=to} -> to
        | _ -> error_type "Only one of `to`,`near_to`,`down_to`,`near_down_to` is allowed."

    inl state = 
        match d with
        | {state} -> state
        | _ -> ()

    inl state = {from state}
    whilecd {
        state
        cond = inl {from state} -> check from
        body = inl {from state} -> {state=body {state i=from}; from=from+by}
        } .state
    |> finally

inl divup a b = (a-1)/b+1 // Integer division with rounding up. (a+b-1)/b is another variant on this.
inl s = span

inl grid_for_template {iteration_mode} {blockDim gridDim} axis dim =
    inl from = threadIdx axis + blockDim axis * blockIdx axis - dim.from
    inl by = gridDim axis * blockDim axis
    inl near_to = dim.near_to

    match iteration_mode with
    | .items_per_thread {d with body} ->
        inl span = s dim
        inl items_per_thread = divup span by
        forcd {d with from=0;near_to=items_per_thread; body=inl {state i=item} ->
            inl i = from + by * item
            inl num_valid = span - by * item
            if i < near_to then body {span num_valid item state i} else state
            }
    | .std d -> forcd {d with from by near_to}

inl grid_for_items = grid_for_template {iteration_mode=.items_per_thread}
inl grid_for = grid_for_template {iteration_mode=.std}
    
inl warp_size = 32
inl syncthreads () = macro.cd unit [text: "__syncthreads()"]

inl cub_block_reduce {d with blockDim redo} x =
    inl ty = 
        match x with
        | @array_is _ -> x.elem_type
        | _ -> type x

    inl algorithm =
        match d with
        | {algorithm} -> algorithm
        | _ -> "BLOCK_REDUCE_WARP_REDUCTIONS"

    inl block_redo = [
        text: "cub::BlockReduce"
        iter: "<",",",">",[type: ty; arg: blockDim.x; text: string_format "cub::{0}" algorithm; arg: blockDim.y; arg: blockDim.z]
        args: ()
        ]

    inl call =
        if eq_type (+) redo then 
            [
            text: ".Sum"
            args:
                match d with
                | {num_valid} -> x,num_valid
                | _ -> x
            ]
        else
            [
            text: ".Reduce"
            args: 
                inl clo = closure_of (inl a,b -> redo a b) ((ty,ty) => ty)
                match d with
                | {num_valid} -> x,clo,num_valid
                | _ -> x,clo
            ]

    macro.cd ty (Tuple.append block_redo call)

inl cub_block_scan {scan_type is_input_tensor return_aggregate} {d with blockDim redo} in =
    inl out, ty = 
        if is_input_tensor then 
            inl elem_type = in.elem_type
            HostTensor.create {
                array_create = array_create_cuda_local
                elem_type dim=in.dim
                }, elem_type
        else array_create_cuda_local in 1 0, type in

    inl ag = if return_aggregate then array_create_cuda_local ty 1 0 else ()
    inl algorithm =
        match d with
        | {algorithm} -> algorithm
        | _ -> "BLOCK_SCAN_RAKING_MEMOIZE"

    inl block_scan =
        [
        text: "cub::BlockScan"
        iter: "<",",",">",[type: ty; arg: blockDim.x; text: string_format "cub::{0}" algorithm; arg: blockDim.y; arg: blockDim.z]
        args: ()
        ]

    inl call =
        inl in, out = if is_input_tensor then in.bodies.ar, out.bodies.ar else in, out

        inl exclusive_scan initial_elem =
            [
            text: ".ExclusiveScan"
            args: 
                inl clo = closure_of (inl a,b -> redo a b) ((ty,ty) => ty)
                if return_aggregate then in,out,initial_elem,clo,ag else in,out,initial_elem,clo
            ]

        if eq_type (+) redo then 
            match scan_type with
            | .inclusive ->
                [
                text: ".InclusiveSum"
                args: if return_aggregate then in,out,ag else in,out
                ]
            | .exclusive, initial_elem ->
                // This is because the exclusive sum does not accept an initial element.
                // The Cub author picked such an uncomfortable place to do this kind of thing in the API.
                exclusive_scan initial_elem 
        else
            match scan_type with
            | .inclusive ->
                [
                text: ".InclusiveScan"
                args: 
                    inl clo = closure_of (inl a,b -> redo a b) ((ty,ty) => ty)
                    if return_aggregate then in,out,clo,ag else in,out,clo
                ]
            | .exclusive, initial_elem ->
                exclusive_scan initial_elem

    macro.cd unit (Tuple.append block_scan call)

    if return_aggregate then 
        inl ag =
            match scan_type with
            | .inclusive -> ag
            | .exclusive, initial_elem -> redo initial_elem ag // For some reason, Cub does not do this on its own.
        out, ag 
    else out

inl cub_warp_reduce redo x =
    macro.cd x [
        text: "cub::WarpReduce"
        types: x
        args: ()
        text: ".Reduce"
        args: x, closure_of (inl a,b -> redo a b) ((x,x) => x)
        ]

inl broadcast_zero x =
    inl ar = array_create_cuda_shared x 1
    if threadIdx.x = 0 then ar 0 <- x
    syncthreads()
    ar 0

inl map' w f (!zip in) (!zip out) =
    assert (in.dim = out.dim) "The input and output dimensions must be equal."
    inl in = flatten in .to_dev_tensor
    inl out = flatten out .to_dev_tensor
    inl in_a :: () = in.dim
    
    inl blockDim = 128
    inl gridDim = min 64 (divup (s in_a) blockDim)

    w.run {
        blockDim gridDim
        kernel = cuda // Lexical scoping rocks.
            grid_for {blockDim gridDim} .x in_a {body=inl {i} ->
                inl out = out i
                inl in = in i
                out .set (f in.get out.get)
                }
        }

inl map w f (!zip in) =
    inl out = w.CudaTensor.create {dim=in.dim; elem_type=type f in.elem_type}
    map' w (inl in _ -> f in) in out
    out

/// The exclusive scan over the innermost dimension.
/// Accepts the optional map_in and map_out arguments for the mapping before the scan and after it.
inl map_d1_exscan_map' w {d with redo neutral_elem} (!zip in) (!zip out) =
    inl dim_in_a, dim_in_b = in.dim
    assert (in.dim = out.dim) "The input and the output dimensions need to be equal"

    inl blockDim = lit_min 1024 (s dim_in_b)
    inl gridDimY = lit_min 64 (s dim_in_a)

    inl in = in.to_dev_tensor
    inl out = out.to_dev_tensor

    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl map_out = match d with {map_out} -> map_out | _ -> const

    w.run {
        blockDim
        gridDim = 1, gridDimY
        kernel = cuda 
            inl grid_for = grid_for {blockDim gridDim}
            grid_for .y dim_in_a {body=inl {i} ->
                inl in, out = in i, out i

                grid_for .x dim_in_b {state=dyn neutral_elem; body=inl {state=prefix i} ->
                    inl in, out = in i, out i
                    inl state, prefix = 
                        cub_block_scan {scan_type=.exclusive,prefix; is_input_tensor=false; return_aggregate=true}
                            {blockDim redo} (map_in in.get)
                    out.set (map_out state out.get)
                    prefix
                    } |> ignore
                }
        }

/// Inclusive scan over the entire tensor.
inl map_inscan_map' w {d with redo neutral_elem} (!zip in) (!zip out) =
    assert (in.dim = out.dim) "The input and output dimensions must be equal."
    inl in = flatten in .to_dev_tensor
    inl out = flatten out .to_dev_tensor
    inl in_a :: () = in.dim

    inl near_to = s in_a
    inl blockDim = lit_min 1024 near_to
    inl num_blocks = divup near_to blockDim
    inl gridDim = lit_min 64 num_blocks

    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl map_out = match d with {map_out} -> map_out | _ -> const

    /// TODO: Optimize the case where the size of temp is just 1.
    inl temp = w.CudaTensor.create {elem_type=type map_in in.elem_type; dim=1,num_blocks}

    inl _ = // First perform the reduction to get the aggregates.
        inl temp = temp 0 .to_dev_tensor
        w.run {
            blockDim gridDim
            kernel = cuda
                grid_for_items {blockDim gridDim} .x in_a {body=inl {num_valid item i} ->
                    inl temp = temp item
                    inl x = in i .get |> map_in |> cub_block_reduce {num_valid blockDim redo}
                    if threadIdx.x = 0 then temp .set x
                    }
            }

    // Scan the aggregates to get the prefixes.
    w.CudaKernel.map_d1_exscan_map' {redo neutral_elem} temp temp

    // The actual scan.
    inl temp = temp 0 .to_dev_tensor
    w.run {
        blockDim gridDim
        kernel = cuda
            grid_for_items {blockDim gridDim} .x in_a {body=inl {num_valid item i} ->
                inl prefix, out = temp item .get, out i
                in i .get 
                |> map_in
                |> cub_block_scan
                    {scan_type=.inclusive; return_aggregate=false; is_input_tensor=false}
                    {blockDim redo}
                |> redo prefix
                |> inl x -> out .set (map_out x out.get)
                }
        }

/// Flattens the tensor to 1d, maps, reduces it and maps it.
/// Map is optional. Allocates a temporary tensor for the intermediary results.
inl map_redo_map w {d with redo neutral_elem} (!zip (!flatten in)) =
    inl run {map_out map_in blockDim gridDim} (!((|>) .to_dev_tensor) in) =
        inl in_a :: () = in.dim
        inl out = w.CudaTensor.create {elem_type=type map_in in.elem_type |> map_out; dim=gridDim}
        inl out' = out.to_dev_tensor

        w.run {
            blockDim gridDim
            kernel = cuda 
                inl x = 
                    grid_for {blockDim gridDim} .x in_a {state=dyn neutral_elem; body=inl {state i} -> redo state (map_in (in i .get)) }
                    |> cub_block_reduce {blockDim redo} |> map_out
                if threadIdx.x = 0 then out' blockIdx.x .set x
            }

        out

    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl map_out = match d with {map_out} -> map_out | _ -> id

    inl in_a :: () = in.dim
    inl span = s in_a
    inl blockDim = lit_min span 256
    inl gridDim = lit_min 64 (divup span blockDim)

    if gridDim = 1 then
        run {map_out map_in blockDim gridDim} in
    else
        run {map_out=id; map_in blockDim gridDim} in
        |> run {map_out map_in=id; blockDim=gridDim; gridDim=1}
    <| 0
        

/// Replicates the 1d `in` and maps it along the outer dimension as determined by in'.
inl d2_replicate_map' w f (!zip in) (!zip in') (!zip out) =
    inl dim_in :: () = in.dim
    inl dim_in'_a, dim_in'_b = in'.dim

    assert (dim_in = dim_in'_b) "Input's dimension must equal the second input's inner dimension."
    assert (in'.dim = out.dim) "Second input must have the same dimension as the output."

    inl blockDimX = min warp_size (s dim_in)
    inl blockDimY = min 32 (s dim_in'_a)
    inl gridDim = min 64 (divup (s dim_in) blockDimX)
    inl in = in.to_dev_tensor
    inl in' = in'.to_dev_tensor
    inl out = out.to_dev_tensor
    
    w.run {
        gridDim
        blockDim=blockDimX,blockDimY
        kernel = cuda 
            inl grid_for = grid_for {gridDim blockDim}
            grid_for .x dim_in'_b {body=inl {i} ->
                inl in = in i
                inl in' j = in' j i 
                inl out j = out j i
                grid_for .y dim_in'_a {body=inl {i} ->
                    inl in', out = in' i, out i
                    out.set (f in.get in'.get out.get)
                    }
                }
        }

inl d2_replicate_map w f (!zip in) in' =
    inl in' =
        match in' with
        | by : int64 -> 
            inl dim_in :: () = in.dim
            HostTensor.create {elem_type=(); dim=by,dim_in}
        | in' -> zip in'
    inl out = w.CudaTensor.create {elem_type=type f in.elem_type in'.elem_type; dim=in'.dim}
    d2_replicate_map' w (inl a b _ -> f a b) in in' out
    out

/// The inclusive scan over the innermost dimension.
/// Accepts the optional map_in and map_out arguments for the mapping before the scan and after it.
inl map_d1_inscan_map' w {d with redo neutral_elem} (!zip in) (!zip out) =
    inl dim_in_a, dim_in_b = in.dim
    assert (in.dim = out.dim) "The input and the output dimensions need to be equal"

    inl blockDim = lit_min 1024 (s dim_in_b)
    inl gridDimY = lit_min 64 (s dim_in_a)

    inl in = in.to_dev_tensor
    inl out = out.to_dev_tensor

    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl map_out = match d with {map_out} -> map_out | _ -> const

    w.run {
        blockDim
        gridDim = 1, gridDimY
        kernel = cuda 
            inl grid_for = grid_for {blockDim gridDim}
            grid_for .y dim_in_a {body=inl {i} ->
                inl in, out = in i, out i

                grid_for .x dim_in_b {state=dyn neutral_elem; body=inl {state=prefix i} ->
                    inl in, out = in i, out i
                    inl state', ag = 
                        cub_block_scan
                            {scan_type=.inclusive; is_input_tensor=false; return_aggregate=true}
                            {blockDim redo} (map_in in.get)
                    out.set (map_out (redo prefix state') out.get)
                    redo prefix ag
                    } |> ignore
                }
        }

/// Maps the two inputs and then reduces the first's inner dimension.
inl map_d1_redo_map' w {d with redo neutral_elem} (!zip in) (!zip in') (!zip out) = 
    inl dim_in_a, dim_in_b = in.dim
    inl dim_in' :: () = in'.dim

    assert (dim_in' = dim_in_a) "Input's outer dimension must equal the output's dimension."
    assert (in'.dim = out.dim) "Input and output's dimensions must be equal."

    inl blockDim = lit_min 1024 (s dim_in_b)
    inl gridDimY = lit_min 64 (s dim_in')

    inl in = in.to_dev_tensor
    inl in' = in'.to_dev_tensor
    inl out = out.to_dev_tensor
    inl map_in = match d with {map_in} -> map_in | _ -> const
    inl map_out = match d with {map_out} -> map_out | _ -> const
        
    w.run {
        blockDim
        gridDim=1,gridDimY
        kernel = cuda 
            inl grid_for = grid_for {blockDim gridDim}
            grid_for .y dim_in_a {body=inl {i} ->
                inl in, in' = in i, in' i

                inl x = 
                    grid_for .x dim_in_b {state=dyn neutral_elem; body=inl {state i} ->
                        inl in = in i 
                        inl a = in.get
                        redo state (map_in a in'.get)
                        }
                    |> cub_block_reduce {blockDim redo}

                if threadIdx.x = 0 then
                    inl out = out i
                    out.set (map_out x out.get)
                }
        }

/// Maps the input and then for every operation in the sequence broadcast maps the reduction over its inner dimensions.
inl map_d1_seq_broadcast' w {d with seq} (!zip in) (!zip out) = 
    inl dim_in_a, dim_in_b = in.dim
    assert (in.dim = out.dim) "The input and the output dimensions need to be equal"

    inl num_valid = s dim_in_b
    inl items_per_thread, blockDim =
        assert (lit_is num_valid) "The inner dimension of the input to this kernel must be known at compile time."
        if num_valid <= 1024 then 1, num_valid
        else divup num_valid 256, 256
    inl gridDimY = min 64 (s dim_in_a)

    inl in = in.to_dev_tensor
    inl out = out.to_dev_tensor

    inl map_in = match d with {map_in} -> map_in | _ -> id
        
    w.run {
        blockDim
        gridDim=1,gridDimY
        kernel = cuda 
            inl dims = {blockDim gridDim}
            grid_for dims .y dim_in_a {body=inl {i} ->
                inl in, out = in i, out i

                inl create_items elem_type = HostTensor.create {
                    array_create = array_create_cuda_local
                    layout=.aot
                    elem_type
                    dim=items_per_thread
                    }

                inl items = create_items (type in.elem_type |> map_in)

                inl inner_loop = grid_for_items dims .x dim_in_b

                inner_loop {body=inl {item i} -> items item .set (in i .get |> map_in)}

                inl rec seq_loop items = function
                    | {s with redo map} :: s' ->
                        inl x = 
                            inl redo = 
                                inl d = {blockDim redo}
                                if num_valid % blockDim.x = 0 then cub_block_reduce d
                                else cub_block_reduce {d with num_valid} 
                            match s with
                            | {map_redo} -> 
                                inl items' = create_items (type map_redo items.elem_type)
                                inner_loop {body=inl {item} -> items item .get |> map_redo |> items' item .set}
                                items'.bodies.ar
                            | _ -> items.bodies.ar
                            |> redo |> broadcast_zero

                        match s' with
                        | () -> 
                            inner_loop {body=inl {item i} ->
                                inl out = out i
                                map (items item .get) x
                                <| out .get |> out .set
                                }
                        | _ ->
                            inl items' = create_items (type map items.elem_type x)
                            inner_loop {body=inl {item i} ->
                                inl out = out i
                                map (items item .get) x
                                |> items' item .set
                                }
                            seq_loop items' s'

                    seq_loop items (Tuple.wrap seq)
                }
        }

inl map_d1_seq_broadcast w {d with seq} (!zip in) =
    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl seq = Tuple.wrap seq
    inl elem_type = type
        inl ty = map_in in.elem_type 
        Tuple.foldl (inl ty {d with map} -> 
            inl ty' = 
                match d with
                | {map_redo} -> map_redo ty
                | _ -> ty
            map ty ty') ty seq
    inl out = w.CudaTensor.create {elem_type dim=in.dim}
    inl rec seq_loop = function
        | {s with map} :: () -> {s with map = inl a b _ -> map a b} :: ()
        | s :: s' -> s :: seq_loop s'
    map_d1_seq_broadcast' w {d with map_in seq=seq_loop seq} in out
    out

/// Maps the two inputs and then scans, maps, reduces and maps the first's inner dimension.
inl mapi_d1_inscan_mapi_d1_reduce_mapi' w {d with scan redo} (!zip in) (!zip in') (!zip out) = 
    inl dim_in_a, dim_in_b = in.dim
    inl dim_in' :: () = in'.dim

    assert (dim_in' = dim_in_a) "Input's outer dimension must equal the output's dimension."
    assert (in'.dim = out.dim) "Input and output's dimensions must be equal."

    inl blockDim = lit_min 1024 (s dim_in_b)
    inl gridDimY = lit_min 64 (s dim_in')

    inl in = in.to_dev_tensor
    inl in' = in'.to_dev_tensor
    inl out = out.to_dev_tensor

    w.run {
        blockDim
        gridDim=1,gridDimY
        kernel = cuda 
            inl grid_for = grid_for {blockDim gridDim}
            grid_for .y dim_in_a {body=inl {i} ->
                inl in = in i
                inl in' = in' i .get

                inl _,redo_prefix =
                    grid_for .x dim_in_b {state=dyn (scan.ne, redo.ne); body=inl {state=scan_prefix,redo_prefix i=j} ->
                        inl in = in j .get
                        inl scan_x, scan_prefix = 
                            match d with
                            | {mapi_in} -> mapi_in i j in in'
                            | {map_in} -> map_in in in'
                            | _ -> in
                            |> cub_block_scan 
                                {scan_type=.inclusive; is_input_tensor=false; return_aggregate=true}
                                {blockDim redo=scan.f}
                            |> Tuple.map (scan.f scan_prefix)
                        inl redo_prefix = 
                            match d with
                            | {mapi_mid} -> mapi_mid i j scan_x in'
                            | {map_mid} -> map_mid scan_x in'
                            | _ -> scan_x
                            |> cub_block_reduce {blockDim redo=redo.f}
                            |> redo.f redo_prefix
                        scan_prefix, redo_prefix
                        }
                if threadIdx.x = 0 then 
                    inl out = out i
                    match d with
                    | {mapi_out} -> map_out i redo_prefix out.get
                    | {map_out} -> map_out redo_prefix out.get
                    | _ -> redo_prefix
                    |> out.set
                }
        }

/// The inclusive scan over the outermost dimension.
/// Accepts the optional map_in and map_out arguments for the mapping before the scan and after it.
inl map_d2_inscan_map' w {d with redo neutral_elem} (!zip in) (!zip out) =
    inl dim_in_a, dim_in_b = in.dim
    assert (in.dim = out.dim) "The input and the output dimensions need to be equal"

    inl blockDimX = lit_min warp_size (s dim_in_b)
    inl blockDimY = lit_min 32 (s dim_in_a)
    inl gridDim = min 64 (divup (s dim_in_b) blockDimX)

    inl in = in.to_dev_tensor
    inl out = out.to_dev_tensor

    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl map_out = match d with {map_out} -> map_out | _ -> const

    w.run {
        gridDim
        blockDim=blockDimX,blockDimY
        kernel = cuda 
            inl grid_for = grid_for {blockDim gridDim}
            grid_for .x dim_in_b {body=inl {i} ->
                inl in j = in j i
                inl out j = out j i

                grid_for .y dim_in_a {state=dyn neutral_elem; body=inl {state=prefix i} -> 
                    inl in, out = in i, out i

                    inl state, prefix = // block inclusive transposed scan
                        inl state = map_in in.get
                        inl near_to = blockDim.y
                        if near_to > 1 then
                            inl from = 1
                            inl to = near_to-from

                            inl ar = 
                                HostTensor.create {
                                    array_create=array_create_cuda_shared
                                    elem_type=state
                                    dim=to, blockDim.x
                                    }
                                |> inl ar i -> ar i threadIdx.x

                            inl {state} =
                                whilecd {
                                    state={from state}
                                    cond=inl {from} -> from < near_to
                                    body=inl {from state} ->
                                        if threadIdx.y < near_to - from then ar threadIdx.y .set state
                                        syncthreads()
                                        inl d = {from=from*2; state}
                                        if threadIdx.y >= from then { d with state = redo self (ar (threadIdx.y-from) .get) }
                                        else d
                                    }
                            inl state = redo prefix state
                            if threadIdx.y = to then ar 0 .set state
                            syncthreads()
                            state, ar 0 .get
                        else
                            inl x = redo prefix state
                            x, x

                    out.set (map_out state out.get)
                    prefix
                    } |> ignore
                }
        }

/// Maps the two inputs and then reduces the first's outer dimension.
inl map_d2_redo_map' w {d with redo neutral_elem} (!zip in) (!zip in') (!zip out) =
    inl dim_in_a, dim_in_b = in.dim
    inl dim_in' :: () = in'.dim

    assert (dim_in' = dim_in_b) "Input's inner dimension must equal the output's dimension."
    assert (in'.dim = out.dim) "Input and output's dimensions must be equal."

    inl blockDimX = lit_min warp_size (s dim_in')
    inl blockDimY = lit_min 32 (s dim_in_a)
    inl gridDim = min 64 (divup (s dim_in') blockDimX)

    inl in = in.to_dev_tensor
    inl in' = in'.to_dev_tensor
    inl out = out.to_dev_tensor
    inl map_in = match d with {map_in} -> map_in | _ -> const
    inl map_out = match d with {map_out} -> map_out | _ -> const

    w.run {
        gridDim
        blockDim=blockDimX,blockDimY
        kernel = cuda 
            inl grid_for = grid_for {blockDim gridDim}
            grid_for .x dim_in_b {body=inl {i} ->
                inl in j = in j i
                inl in' = in' i
                inl out = out i
                inl finally result = out.set (map_out result out.get)

                inl state = 
                    grid_for .y dim_in_a {state=dyn neutral_elem; body=inl {state i} -> 
                        inl in = in i 
                        redo state (map_in in.get in'.get) 
                        }
                        
                if blockDim.y > 1 then
                    inl near_to = blockDim.y
                    inl ar = 
                        HostTensor.create {
                            array_create=array_create_cuda_shared
                            elem_type=state
                            dim={from=1; near_to}, blockDim.x
                            }
                        |> inl ar i -> ar i threadIdx.x

                    whilecd {
                        state={near_to state}
                        cond=inl {near_to} -> near_to >= 2
                        body=inl {near_to state} ->
                            inl by = near_to/2 // It might be worth trying `max 1 (near_to/3)`
                            if threadIdx.y < near_to && threadIdx.y >= by then ar threadIdx.y .set state
                            syncthreads()

                            {
                            near_to=by 
                            state=
                                if threadIdx.y < by then
                                    forcd {from=threadIdx.y+by; by near_to state 
                                        body=inl {state i} -> redo state (ar i .get)
                                        }
                                else
                                    state
                            }
                        }
                    |> inl {state} -> if threadIdx.y = 0 then finally state
                else
                    finally state
            }
        } |> ignore

inl map_dx_redo_map_template dim kernel w d in in' =
    inl in' = 
        match in' with
        | () -> HostTensor.create {elem_type=(); dim}
        | in' -> zip in'

    inl map_in = match d with {map_in} -> map_in | _ -> const
    inl map_out, elem_type = 
        inl ty = type map_in in.elem_type in'.elem_type
        match d with {map_out} -> (inl a _ -> map_out a),(type map_out ty) | _ -> const, ty
    inl out = w.CudaTensor.create {elem_type dim=in'.dim}
    kernel w {d with map_in map_out} in in' out
    out

inl map_d1_redo_map w d (!zip in) = map_dx_redo_map_template (fst in.dim) map_d1_redo_map' w d in
inl map_d2_redo_map w d (!zip in) = map_dx_redo_map_template (snd in.dim) map_d2_redo_map' w d in

inl map_dx_scan_map_template kernel w d (!zip in) =
    inl map_in = match d with {map_in} -> map_in | _ -> id
    inl map_out, elem_type = 
        inl ty = type map_in in.elem_type
        match d with {map_out} -> (inl a _ -> map_out a), (type map_out ty) | _ -> const, ty
    inl out = w.CudaTensor.create {elem_type dim=in.dim}
    kernel w {d with map_in map_out} in out
    out

inl map_d1_exscan_map = map_dx_scan_map_template map_d1_exscan_map'
inl map_d1_inscan_map = map_dx_scan_map_template map_d1_inscan_map'
inl map_d2_inscan_map = map_dx_scan_map_template map_d2_inscan_map'
inl map_inscan_map = map_dx_scan_map_template map_inscan_map'

inl mapi_d1_inscan_mapi_d1_reduce_mapi w d (!zip in) in' =
    inl in' = 
        match in' with
        | () -> HostTensor.create {elem_type=(); dim=fst in.dim}
        | in' -> zip in'

    inl elem_type = type
        inl in = in.elem_type 
        inl in' = in'.elem_type
        match d with
        | {mapi_in} -> mapi_in 0 0 in in'
        | {map_in} -> map_in in in'
        | _ -> in
        |>
        match d with
        | {mapi_mid} x -> mapi_mid 0 0 x in'
        | {map_mid} x -> map_mid x in'
        | _ -> id
        |>
        match d with
        | {mapi_out} -> mapi_out 0
        | {map_out} -> map_out
        | _ -> id

    inl d =
        match d with
        | {mapi_out} -> {d with mapi_out=inl i x _ -> mapi_out i x}
        | {map_out} -> {d with map_out=inl x _ -> map_out x}
        | _ -> {d with map_out=const}
        
    inl out = w.CudaTensor.create {elem_type dim=in'.dim}
    mapi_d1_inscan_mapi_d1_reduce_mapi' w d in in' out
    out

/// Creates a tensor using the given generator function.
/// Takes in the optional {thread_limit} as the first argument in order to control the degree of parallelism.
inl init' w d f (!zip out) =
    inl out = out.to_dev_tensor

    inl dim = out.dim
    inl rec merge = function
        | thread_limit :: l', dim :: d' -> {dim thread_limit} :: merge (l', d')
        | (), d' -> Tuple.map (inl dim -> {dim thread_limit=()}) d'
    inl d = 
        match d with
        | {thread_limit} -> merge (Tuple.wrap thread_limit,dim)
        | {rev_thread_limit} -> merge (Tuple.wrap rev_thread_limit,Tuple.rev dim) |> Tuple.rev
        | _ -> merge ((),dim)
    inl s = function {thread_limit=() dim} -> s dim | {thread_limit} -> thread_limit
    inl near_to = Tuple.foldl (inl a (!s b) -> a*b) 1 d
    inl blockDim = min near_to 256
    inl gridDim = divup near_to blockDim

    w.run {blockDim gridDim
        kernel = cuda
            grid_for {blockDim gridDim} .x {from=0; near_to} {body=inl {i} ->
                inl l,_ = Tuple.foldr (inl ((!s x_span) & x) (l,i) -> (i % x_span - x.dim.from) :: l, i / x_span) d ((),i)
                inl rec loop f out = function
                    | {thread_limit=()} :: d', i :: i' -> loop (f i) (out i) (d', i')
                    | {thread_limit=by dim={near_to}} :: d', from :: i' -> forcd {from by near_to body=inl {i} -> loop (f i) (out i) (d',i')}
                    | (), () -> out.set f
                loop f out (d,l)
                }
        }

inl init w {d with dim} f =
    inl dim = Tuple.wrap dim
    inl elem_type = type Tuple.foldl (inl f _ -> f 0) f dim
    inl out = w.CudaTensor.create {dim elem_type}
    init' w d f out
    out

inl methods =
    {
    map' map map_redo_map d2_replicate_map' d2_replicate_map map_d1_redo_map' map_d1_redo_map map_d2_redo_map' map_d2_redo_map
    map_d1_inscan_map' map_d1_inscan_map map_d2_inscan_map' map_d2_inscan_map map_inscan_map' map_inscan_map 
    map_d1_exscan_map' map_d1_exscan_map mapi_d1_inscan_mapi_d1_reduce_mapi' mapi_d1_inscan_mapi_d1_reduce_mapi
    map_d1_seq_broadcast' map_d1_seq_broadcast init' init
    }

inl s -> s.module_add .CudaKernel methods
    """) |> module_

let cuda_modules =
    (
    "CudaModules",[cuda;allocator;region;cuda_stream;cuda_tensor;cuda_kernel;cuda_random;cuda_blas;console],"All the cuda modules in one.",
    """
inl size ret ->
    inb s = Cuda
    inb s = Allocator s size
    inb s = CudaRandom s
    inb s = CudaBlas s
    inl s = Region s |> CudaStream |> CudaTensor |> CudaKernel
    inb s = s.RegionMem.create'
    inb s = s.RegionStream.create'
    ret s
    """) |> module_

let learning =
    (
    "Learning",[host_tensor;extern_],"The deep learning module.",
    """
inl float s ->
    open HostTensor

    // #Primitives
    inl zero = to float 0
    inl one = to float 1
    inl two = to float 2
    inl infinity =
        match float with
        | _: float32 -> infinityf32
        | _: float64 -> infinityf64

    inl primal = function {primal} | primal -> primal
    inl adjoint = function {adjoint} -> adjoint | _ -> .nil

    inl primals = toa_map primal
    inl adjoints = toa_map adjoint

    inl is_not_unit = function
        | () -> false
        | _ -> true

    inl rec filter_units = function
        | x :: x' -> 
            match filter_units x with
            | () -> filter_units x'
            | x -> x :: filter_units x'
        | {} & x ->
            module_filter (inl k (!filter_units (!is_not_unit x)) -> x) x
            |> inl x -> if eq_type x {} then () else x
        | .nil -> ()
        | x -> x

    // What this does is selectively filter out the results of applying f 
    // where the adjoints are missing (in other words constants.)
    inl filter_based_on_adjoints x adjoint =
        inl rec mark_units = function
            | x :: x', y :: y' -> mark_units (x,y) :: mark_units (x',y')
            | (), () -> ()
            | (), _ | _, () -> error_type "Tuple dimesions do not match."
            | {} & x, {} & y -> module_map (inl k y -> mark_units (x k,y)) y
            | _, .nil -> ()
            | x, _ -> x
        mark_units (x, adjoint) |> filter_units

    inl filter_unit_and_branch x ret =
        match filter_units x with
        | () -> ()
        | x -> ret x

    inl on_non_nil B ret =
        match B with
        | .nil -> ()
        | B -> ret B

    inl dr s primal = {primal adjoint=s.CudaTensor.zero_like primal; block_toa_map=()}

    inl s = s.member_add .dr dr

    inl matmult A B s =
        inl C = s.CudaBlas.gemm .nT .nT one (primal A) (primal B) |> s.dr
        C, inl _ ->
            on_non_nil (adjoint A) (inl A -> s.CudaBlas.gemm' .nT .T one (adjoint C) (primal B) one A)
            on_non_nil (adjoint B) (inl B -> s.CudaBlas.gemm' .T .nT one (primal A) (adjoint C) one B)

    inl map {fwd bck} in s =
        inl primal, adjoint = primals in, adjoints in
        inl out = s.CudaKernel.map fwd primal |> s.dr
        out, inl _ ->
            inl out = match out with {primal adjoint} -> zip (primal, adjoint) .update_body2 (inl P A -> {P A})
            inl bck =
                inl bck = filter_based_on_adjoints bck adjoint
                inl in adjoint -> toa_map ((|>) in) bck |> toa_map2 (+) adjoint

            inb adjoint = filter_unit_and_branch adjoint 
            s.CudaKernel.map' bck {in=primal; out} adjoint

    inl map_redo_map {fwd bck} in s =
        inl primal, adjoint = primals in, adjoints in
        inl out = s.CudaKernel.map_redo_map fwd primal |> s.dr
        
        out, inl _ ->
            inl out = match out with {primal adjoint} -> zip (primal, adjoint) .update_body2 (inl P A -> {P A}) .to_dev_tensor
            inl bck =
                inl bck = filter_based_on_adjoints bck adjoint
                inl {in} adjoint -> toa_map ((|>) {in out=out.get}) bck |> toa_map2 (+) adjoint
            inb adjoint = filter_unit_and_branch adjoint 
            s.CudaKernel.map' bck {in=primal} adjoint

    inl d2_replicate_map {fwd bck={bck_in bck_in'}} in in' s =
        inl primal, adjoint = primals in, adjoints in
        inl primal', adjoint' = primals in', adjoints in'
        inl out = s.CudaKernel.d2_replicate_map fwd primal primal' |> s.dr
        out, inl _ ->
            inl out = match out with {primal adjoint} -> zip (primal, adjoint) .update_body2 (inl P A -> {P A})
            on_non_nil adjoint (s.CudaKernel.map_d2_redo_map' bck_in {in'=primal'; out} primal)
            on_non_nil adjoint' (s.CudaKernel.d2_replicate_map' bck_in' primal {in'=primal'; out})

    inl matmultb l bias s =
        inl l =
            match l with
            | () -> error_type "First argument must not be empty."
            | (_,_) :: _ -> l
            | _ :: _ -> l :: ()
        inl C = 
            Tuple.foldl (inl C (A,B) ->
                match C with
                | () -> s.CudaBlas.gemm .nT .nT one (primal A) (primal B) |> s.dr
                | C -> s.CudaBlas.gemm' .nT .nT one (primal A) (primal B) one (primal C)
                ) () l
        s.CudaKernel.d2_replicate_map' (inl a b _ -> a+b) (primal bias) (primal C) (primal C)
        C, inl _ ->
            inl C' = adjoint C
            Tuple.iter (inl A, B ->
                on_non_nil (adjoint A) (inl A -> s.CudaBlas.gemm' .nT .T one C' (primal B) one A)
                on_non_nil (adjoint B) (inl B -> s.CudaBlas.gemm' .T .nT one (primal A) C' one B)
                ) l
            on_non_nil (adjoint bias) (inl bias -> s.CudaKernel.map_d2_redo_map' {map_in=const;neutral_elem=zero;redo=(+);map_out=(+)} C' bias.empty bias)

    inl add_bias = d2_replicate_map {
        fwd=(+)
        bck={
            bck_in={
                map_in=inl {out} _ -> out.A
                neutral_elem=zero;redo=(+)
                map_out=(+)
                }
            bck_in'=inl _ {out} adjoint -> out.A + adjoint
            }
        }

    inl Primitive =
        {
        matmult map map_redo_map add_bias matmultb d2_replicate_map
        } |> stack

    // #Operations
    inl (>>=) a b s =
        inl a,a_bck = a s
        inl b,b_bck = b a s
        b, inl _ -> b_bck(); a_bck()

    inl succ x _ = x, const ()

    inl multiply_by_adjoint f {d with out={A P} in} = toa_map ((*) A) (f {in out=P})
    inl activation d = map {d with bck = multiply_by_adjoint self }

    inl sigmoid = activation {
        fwd = inl x -> one / (one + exp -x)
        bck = inl {out} -> out * (one - out)
        }

    inl Activation = {activation sigmoid} |> stack

    //#Error
    inl accuracy (input,label) s =
        inl input, label = primal input, primal label
        inl _ ->
            inl x = 
                s.CudaKernel.map_d1_redo_map {
                    map_in=const
                    neutral_elem=-infinity,zero
                    redo=inl a b -> if fst a > fst b then a else b
                    map_out=snd
                    } (input,label) ()
            Array.foldl (inl s x -> if x = one then s+1 else s) (dyn 0) (s.CudaTensor.to_host_tensor x).bodies.ar 
        , const ()

    inl error {fwd bck} (input,_ as x) = 
        inl batch_size = primal input .span_outer |> to float
        inl div_by_minibatch_size x = x / batch_size
        inm cost =
            map_redo_map {
                fwd = {
                    map_in = fwd
                    redo = (+)
                    neutral_elem = zero
                    map_out = div_by_minibatch_size
                    }
                bck = toa_map (inl bck -> multiply_by_adjoint bck >> div_by_minibatch_size) bck
                } x
        inm accuracy = accuracy x
        succ {cost accuracy}

    inl square = error {
        fwd = inl (x,y) -> (y - x) * (y - x)
        bck = 
            inl {in=x,y} -> two * (x - y)
            ,inl {in=x,y} -> two * (y - x)
        }

    inl cross_entropy = error {
        fwd = inl x, y -> -(y * log x + (one - y) * log (one - x))
        bck = 
            inl {in=x, y} -> (x - y) / (x * (one - x))
            ,inl {in=x, y} -> log (one - x) - log x
        }

    inl Error = {accuracy error square cross_entropy} |> stack

    // #Feedforward
    inl layer initializer activation hidden_size input_size s =
        inl weight = initializer (input_size, hidden_size) s |> s.dr
        inl bias = s.CudaTensor.zero {elem_type=float; dim=hidden_size} |> s.dr
        {
        hidden_size
        weights = weight, bias
        apply = inl input -> matmultb (input, weight) bias >>= activation
        }

    inl rec init layers input_size s = 
        match layers with
        | x :: x' ->
            inl {hidden_size weights apply} = init x input_size s
            inl x' = init x' hidden_size s
            {x' with weights=weights :: self; apply = apply >>= self}
        | () -> {hidden_size=input_size; weigths=(); apply=succ}
        | x -> x input_size s

    inl with_error error network = {network with apply = inl (input,label) -> self input >>= inl input -> error (input,label)}

    inl sigmoid_initializer dim s = 
        inl stddev = sqrt (two / to float (Tuple.foldl (+) 0 dim))
        s.CudaRandom.create {dst=.Normal; stddev mean=0.0f32} {dim elem_type=type zero}

    inl sigmoid = layer sigmoid_initializer sigmoid
    inl linear = layer sigmoid_initializer succ

    inl Layer = {sigmoid linear} |> stack

    inl run {d with network={weights apply} input label state=!dyn state} s =
        inl dim1 x = x.dim |> fst
        open Extern
        open Console

        assert (dim1 input = dim1 label) "Training and test set need to have to equal first dimensions."

        inl optimizer =
            match d with // Take care not to pass d in by accident into run_minibatch.
            | {optimizer} {cost},bck ->
                s.CudaTensor.set (adjoint cost) one
                bck() // Runs the backwards pass.
                toa_iter optimizer weights
            | _ _ -> ()

        inl run_minibatch {state input label} = 
            s.Section.allocate.refresh
            inb s = s.RegionMem.create'
            inl {cost accuracy}, _ as er = apply (input, label) s
            optimizer er

            inl running_cost =
                match state with
                | {running_cost} -> running_cost + to float64 (primal cost |> s.CudaTensor.get) * to float64 input.span_outer
                
            match state with
            | {running_accuracy} -> { running_cost running_accuracy=running_accuracy + accuracy () }
            | _ -> {running_cost}
            
        inl {from near_to} = dim1 input
        inl span = near_to - from
        inl by = match d with {minibatch_size} -> minibatch_size | _ -> span

        inl state = Loops.for' {from near_to; state by; body=inl {next state i=from} ->
            if macro.fs bool [text: "System.Double.IsNaN"; args: state.running_cost] then
                state
            else
                inl span = if span % by = 0 then {from by} else {from near_to=from+by |> min near_to} 
                inl f x = x.view_span (const span)
                run_minibatch {state input=f input; label=f label}
                |> next
            }

        writeline "-----"
        writeline "Batch done."
        inl spanf64 = to float64 span
        inl cost = 
            match state with 
            | {running_cost} -> 
                inl cost = running_cost / spanf64
                string_format "Average of batch costs is {0}." cost |> writeline 
                cost
            | _ -> ()
        match state with 
        | {running_accuracy} -> 
            inl percetange = to float64 running_accuracy / spanf64 * 100f64
            string_format "The accuracy of the batch is {0}/{1}({2}%). " (running_accuracy,span,percetange) |> writeline 
        | _ -> ()
        writeline "-----"
        cost

    inl Feedforward = {Layer init with_error run} |> stack

    // #Optimizer
    inl sgd learning_rate x = 
        inl primal, adjoint = primal x, adjoint x
        s.CudaKernel.map' (toa_map2 (inl A P -> P - learning_rate * A)) adjoint primal
        s.CudaTensor.clear adjoint 

    inl Optimizer = {sgd}

    inl grad_check {d with network={weights apply} input label} s =
        open Extern

        inl run () = 
            inl {cost accuracy}, bck = apply (input, label) s
            s.CudaTensor.set (adjoint cost) one
            bck()
        met cost () =
            inl {cost accuracy}, bck = apply (input, label) s
            s.CudaTensor.get (primal cost)
        //met update () = 
        //    toa_iter (sgd (to float 0.01)) weights

        run()

        inl epsilon = to float 0.001
        inl boundary = to float 0.001
        
        inl rec perturb primal adjoint =
            assert (primal.dim = adjoint.dim) "Dimensions must be equal."
            match primal.dim with
            | {from near_to} :: _ ->
                Loops.for {from near_to body=inl {i} ->
                    perturb (primal i) (adjoint i)
                    }
            | _ -> 
                inl orig = s.CudaTensor.get primal
                s.CudaTensor.set primal (orig + epsilon)
                inl cost_plus_epsilon = cost ()
                s.CudaTensor.set primal(orig - epsilon)
                inl cost_minus_epsilon = cost ()
                s.CudaTensor.set primal orig
                inl approx_gradient = (cost_plus_epsilon - cost_minus_epsilon) / (2.0f32 * epsilon)

                inl true_gradient = s.CudaTensor.get adjoint
                
                inl diff = abs (true_gradient - approx_gradient)
                if diff >= boundary then
                    Console.writeline {true_gradient approx_gradient diff}
                    Console.writeline "--- Gradient checking failure."
                
        toa_iter (inl t -> perturb (primal t) (adjoint t)) weights

    { primal primals adjoint adjoints (>>=) succ Primitive Activation Error Feedforward Optimizer grad_check s }
    """) |> module_
