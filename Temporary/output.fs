module SpiralExample.Main
let cuda_kernels = """
#include "cub/cub.cuh"

extern "C" {
    typedef float(*FunPointer0)(float, float);
    __global__ void method_7(float * var_0, float * var_1);
    __global__ void method_9(float * var_0, float * var_1, float * var_2);
    __global__ void method_17(float var_0, float var_1, float * var_2, float * var_3, float * var_4, float * var_5);
    __global__ void method_19(float * var_0, float * var_1, float * var_2, float * var_3, float * var_4);
    __device__ void method_8(float * var_0, float * var_1, long long int var_2);
    __device__ float method_10(float * var_0, float * var_1, float var_2, long long int var_3);
    __device__ float method_11(float var_0, float var_1);
    __device__ void method_18(float var_0, float var_1, float * var_2, float * var_3, float * var_4, float * var_5, long long int var_6);
    __device__ void method_20(float * var_0, float * var_1, float * var_2, float * var_3, float * var_4, long long int var_5);
    
    __global__ void method_7(float * var_0, float * var_1) {
        long long int var_2 = threadIdx.x;
        long long int var_3 = threadIdx.y;
        long long int var_4 = threadIdx.z;
        long long int var_5 = blockIdx.x;
        long long int var_6 = blockIdx.y;
        long long int var_7 = blockIdx.z;
        long long int var_8 = (var_5 * 128);
        long long int var_9 = (var_8 + var_2);
        method_8(var_0, var_1, var_9);
    }
    __global__ void method_9(float * var_0, float * var_1, float * var_2) {
        long long int var_3 = threadIdx.x;
        long long int var_4 = threadIdx.y;
        long long int var_5 = threadIdx.z;
        long long int var_6 = blockIdx.x;
        long long int var_7 = blockIdx.y;
        long long int var_8 = blockIdx.z;
        long long int var_9 = (var_6 * 128);
        long long int var_10 = (var_9 + var_3);
        float var_11 = 0;
        float var_12 = method_10(var_0, var_1, var_11, var_10);
        FunPointer0 var_15 = method_11;
        float var_16 = cub::BlockReduce<float,128>().Reduce(var_12, var_15);
        char var_17 = (var_3 == 0);
        if (var_17) {
            char var_18 = (var_6 >= 0);
            char var_20;
            if (var_18) {
                var_20 = (var_6 < 1);
            } else {
                var_20 = 0;
            }
            char var_21 = (var_20 == 0);
            if (var_21) {
                // unprinted assert;
            } else {
            }
            var_2[var_6] = var_16;
        } else {
        }
    }
    __global__ void method_17(float var_0, float var_1, float * var_2, float * var_3, float * var_4, float * var_5) {
        long long int var_6 = threadIdx.x;
        long long int var_7 = threadIdx.y;
        long long int var_8 = threadIdx.z;
        long long int var_9 = blockIdx.x;
        long long int var_10 = blockIdx.y;
        long long int var_11 = blockIdx.z;
        long long int var_12 = (var_9 * 128);
        long long int var_13 = (var_12 + var_6);
        method_18(var_0, var_1, var_2, var_3, var_4, var_5, var_13);
    }
    __global__ void method_19(float * var_0, float * var_1, float * var_2, float * var_3, float * var_4) {
        long long int var_5 = threadIdx.x;
        long long int var_6 = threadIdx.y;
        long long int var_7 = threadIdx.z;
        long long int var_8 = blockIdx.x;
        long long int var_9 = blockIdx.y;
        long long int var_10 = blockIdx.z;
        long long int var_11 = (var_8 * 128);
        long long int var_12 = (var_11 + var_5);
        method_20(var_0, var_1, var_2, var_3, var_4, var_12);
    }
    __device__ void method_8(float * var_0, float * var_1, long long int var_2) {
        char var_3 = (var_2 < 8);
        if (var_3) {
            char var_4 = (var_2 >= 0);
            char var_5 = (var_4 == 0);
            if (var_5) {
                // unprinted assert;
            } else {
            }
            if (var_5) {
                // unprinted assert;
            } else {
            }
            float var_6 = var_0[var_2];
            float var_7 = (-var_6);
            float var_8 = exp(var_7);
            float var_9 = (1 + var_8);
            float var_10 = (1 / var_9);
            var_1[var_2] = var_10;
            long long int var_11 = (var_2 + 128);
            method_8(var_0, var_1, var_11);
        } else {
        }
    }
    __device__ float method_10(float * var_0, float * var_1, float var_2, long long int var_3) {
        char var_4 = (var_3 < 8);
        if (var_4) {
            char var_5 = (var_3 >= 0);
            char var_6 = (var_5 == 0);
            if (var_6) {
                // unprinted assert;
            } else {
            }
            float var_7 = var_0[var_3];
            float var_8 = var_1[var_3];
            float var_9 = (var_8 - var_7);
            float var_10 = (var_9 * var_9);
            float var_11 = (var_2 + var_10);
            long long int var_12 = (var_3 + 128);
            return method_10(var_0, var_1, var_11, var_12);
        } else {
            return var_2;
        }
    }
    __device__ float method_11(float var_0, float var_1) {
        return (var_0 + var_1);
    }
    __device__ void method_18(float var_0, float var_1, float * var_2, float * var_3, float * var_4, float * var_5, long long int var_6) {
        char var_7 = (var_6 < 8);
        if (var_7) {
            char var_8 = (var_6 >= 0);
            char var_9 = (var_8 == 0);
            if (var_9) {
                // unprinted assert;
            } else {
            }
            if (var_9) {
                // unprinted assert;
            } else {
            }
            float var_10 = var_2[var_6];
            float var_11 = var_3[var_6];
            float var_12 = var_4[var_6];
            float var_13 = (var_11 - var_12);
            float var_14 = (2 * var_13);
            float var_15 = (var_0 * var_14);
            float var_16 = (var_10 + var_15);
            var_5[var_6] = var_16;
            long long int var_17 = (var_6 + 128);
            method_18(var_0, var_1, var_2, var_3, var_4, var_5, var_17);
        } else {
        }
    }
    __device__ void method_20(float * var_0, float * var_1, float * var_2, float * var_3, float * var_4, long long int var_5) {
        char var_6 = (var_5 < 8);
        if (var_6) {
            char var_7 = (var_5 >= 0);
            char var_8 = (var_7 == 0);
            if (var_8) {
                // unprinted assert;
            } else {
            }
            if (var_8) {
                // unprinted assert;
            } else {
            }
            float var_9 = var_0[var_5];
            float var_10 = var_1[var_5];
            float var_11 = var_2[var_5];
            float var_12 = var_3[var_5];
            float var_13 = (1 - var_12);
            float var_14 = (var_12 * var_13);
            float var_15 = (var_11 * var_14);
            float var_16 = (var_9 + var_15);
            var_4[var_5] = var_16;
            long long int var_17 = (var_5 + 128);
            method_20(var_0, var_1, var_2, var_3, var_4, var_17);
        } else {
        }
    }
}
"""

type Union0 =
    | Union0Case0 of Tuple1
    | Union0Case1
and Tuple1 =
    struct
    val mem_0: ManagedCuda.BasicTypes.CUdeviceptr
    new(arg_mem_0) = {mem_0 = arg_mem_0}
    end
and Env2 =
    struct
    val mem_0: Env3
    val mem_1: int64
    new(arg_mem_0, arg_mem_1) = {mem_0 = arg_mem_0; mem_1 = arg_mem_1}
    end
and Env3 =
    struct
    val mem_0: (Union0 ref)
    new(arg_mem_0) = {mem_0 = arg_mem_0}
    end
and Union4 =
    | Union4Case0 of Tuple5
    | Union4Case1
and Tuple5 =
    struct
    val mem_0: float32
    new(arg_mem_0) = {mem_0 = arg_mem_0}
    end
let rec method_0 ((var_0: System.Diagnostics.DataReceivedEventArgs)): unit =
    let (var_1: string) = var_0.get_Data()
    let (var_2: string) = System.String.Format("{0}",var_1)
    System.Console.WriteLine(var_2)
and method_1((var_0: (Union0 ref))): ManagedCuda.BasicTypes.CUdeviceptr =
    let (var_1: Union0) = (!var_0)
    match var_1 with
    | Union0Case0(var_2) ->
        var_2.mem_0
    | Union0Case1 ->
        (failwith "A Cuda memory cell that has been disposed has been tried to be accessed.")
and method_2((var_0: uint64), (var_1: System.Collections.Generic.Stack<Env2>), (var_2: uint64), (var_3: int64)): Env3 =
    let (var_4: int32) = var_1.get_Count()
    let (var_5: bool) = (var_4 > 0)
    if var_5 then
        let (var_6: Env2) = var_1.Peek()
        let (var_7: Env3) = var_6.mem_0
        let (var_8: int64) = var_6.mem_1
        let (var_9: (Union0 ref)) = var_7.mem_0
        let (var_10: Union0) = (!var_9)
        match var_10 with
        | Union0Case0(var_11) ->
            let (var_12: ManagedCuda.BasicTypes.CUdeviceptr) = var_11.mem_0
            method_3((var_12: ManagedCuda.BasicTypes.CUdeviceptr), (var_0: uint64), (var_2: uint64), (var_3: int64), (var_1: System.Collections.Generic.Stack<Env2>), (var_7: Env3), (var_8: int64))
        | Union0Case1 ->
            let (var_14: Env2) = var_1.Pop()
            let (var_15: Env3) = var_14.mem_0
            let (var_16: int64) = var_14.mem_1
            method_2((var_0: uint64), (var_1: System.Collections.Generic.Stack<Env2>), (var_2: uint64), (var_3: int64))
    else
        method_4((var_0: uint64), (var_2: uint64), (var_3: int64), (var_1: System.Collections.Generic.Stack<Env2>))
and method_5((var_0: (Union0 ref))): ManagedCuda.BasicTypes.CUdeviceptr =
    let (var_1: Union0) = (!var_0)
    match var_1 with
    | Union0Case0(var_2) ->
        var_2.mem_0
    | Union0Case1 ->
        (failwith "A Cuda memory cell that has been disposed has been tried to be accessed.")
and method_6((var_0: ManagedCuda.CudaBlas.CudaBlasHandle), (var_1: int32), (var_2: int32), (var_3: int32), (var_4: float32), (var_5: (Union0 ref)), (var_6: int64), (var_7: int64), (var_8: int64), (var_9: int64), (var_10: int64), (var_11: int64), (var_12: int64), (var_13: int64), (var_14: int32), (var_15: (Union0 ref)), (var_16: int64), (var_17: int64), (var_18: int64), (var_19: int64), (var_20: int64), (var_21: int64), (var_22: int64), (var_23: int64), (var_24: int32), (var_25: float32), (var_26: (Union0 ref)), (var_27: int64), (var_28: int64), (var_29: int64), (var_30: int64), (var_31: int64), (var_32: int64), (var_33: int64), (var_34: int64), (var_35: int32)): unit =
    let (var_36: ManagedCuda.CudaBlas.Operation) = ManagedCuda.CudaBlas.Operation.NonTranspose
    let (var_37: ManagedCuda.CudaBlas.Operation) = ManagedCuda.CudaBlas.Operation.NonTranspose
    let (var_38: (float32 ref)) = (ref var_4)
    let (var_39: bool) = (var_10 < var_11)
    let (var_40: bool) = (var_39 = false)
    if var_40 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_41: bool) = (var_12 < var_13)
    let (var_42: bool) = (var_41 = false)
    if var_42 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_43: int64) = (var_13 - var_12)
    let (var_44: int64) = (var_11 - var_10)
    let (var_45: int64) = (var_44 * var_43)
    let (var_46: bool) = (0L = var_7)
    let (var_47: bool) = (var_46 = false)
    if var_47 then
        (failwith "The inner dimensions much have offsets of 0. They must not be 'view'ed. Consider reshaping a copy of the tensor instead")
    else
        ()
    let (var_48: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_5: (Union0 ref)))
    let (var_49: ManagedCuda.BasicTypes.SizeT) = var_48.Pointer
    let (var_50: uint64) = uint64 var_49
    let (var_51: uint64) = (uint64 var_6)
    let (var_52: uint64) = (var_50 + var_51)
    let (var_53: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_52)
    let (var_54: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_53)
    let (var_55: bool) = (var_20 < var_21)
    let (var_56: bool) = (var_55 = false)
    if var_56 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_57: bool) = (var_22 < var_23)
    let (var_58: bool) = (var_57 = false)
    if var_58 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_59: int64) = (var_23 - var_22)
    let (var_60: int64) = (var_21 - var_20)
    let (var_61: int64) = (var_60 * var_59)
    let (var_62: bool) = (0L = var_17)
    let (var_63: bool) = (var_62 = false)
    if var_63 then
        (failwith "The inner dimensions much have offsets of 0. They must not be 'view'ed. Consider reshaping a copy of the tensor instead")
    else
        ()
    let (var_64: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_15: (Union0 ref)))
    let (var_65: ManagedCuda.BasicTypes.SizeT) = var_64.Pointer
    let (var_66: uint64) = uint64 var_65
    let (var_67: uint64) = (uint64 var_16)
    let (var_68: uint64) = (var_66 + var_67)
    let (var_69: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_68)
    let (var_70: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_69)
    let (var_71: (float32 ref)) = (ref var_25)
    let (var_72: bool) = (var_31 < var_32)
    let (var_73: bool) = (var_72 = false)
    if var_73 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_74: bool) = (var_33 < var_34)
    let (var_75: bool) = (var_74 = false)
    if var_75 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_76: int64) = (var_34 - var_33)
    let (var_77: int64) = (var_32 - var_31)
    let (var_78: int64) = (var_77 * var_76)
    let (var_79: bool) = (0L = var_28)
    let (var_80: bool) = (var_79 = false)
    if var_80 then
        (failwith "The inner dimensions much have offsets of 0. They must not be 'view'ed. Consider reshaping a copy of the tensor instead")
    else
        ()
    let (var_81: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_26: (Union0 ref)))
    let (var_82: ManagedCuda.BasicTypes.SizeT) = var_81.Pointer
    let (var_83: uint64) = uint64 var_82
    let (var_84: uint64) = (uint64 var_27)
    let (var_85: uint64) = (var_83 + var_84)
    let (var_86: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_85)
    let (var_87: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_86)
    let (var_88: ManagedCuda.CudaBlas.CublasStatus) = ManagedCuda.CudaBlas.CudaBlasNativeMethods.cublasSgemm_v2(var_0, var_36, var_37, var_1, var_2, var_3, var_38, var_54, var_14, var_70, var_24, var_71, var_87, var_35)
    if var_88 <> ManagedCuda.CudaBlas.CublasStatus.Success then raise <| new ManagedCuda.CudaBlas.CudaBlasException(var_88)
and method_16((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_2: (Union4 ref)), (var_3: (Union4 ref))): float32 =
    let (var_4: Union4) = (!var_3)
    match var_4 with
    | Union4Case0(var_5) ->
        var_5.mem_0
    | Union4Case1 ->
        let (var_7: float32) = method_14((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_2: (Union4 ref)))
        var_3 := (Union4Case0(Tuple5(var_7)))
        var_7
and method_15((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_2: (Union4 ref))): float32 =
    let (var_3: Union4) = (!var_2)
    match var_3 with
    | Union4Case0(var_4) ->
        var_4.mem_0
    | Union4Case1 ->
        let (var_6: float32) = method_12((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext))
        var_2 := (Union4Case0(Tuple5(var_6)))
        var_6
and method_21((var_0: ManagedCuda.CudaBlas.CudaBlasHandle), (var_1: int32), (var_2: int32), (var_3: int32), (var_4: float32), (var_5: (Union0 ref)), (var_6: int64), (var_7: int64), (var_8: int64), (var_9: int64), (var_10: int64), (var_11: int64), (var_12: int64), (var_13: int64), (var_14: int32), (var_15: (Union0 ref)), (var_16: int64), (var_17: int64), (var_18: int64), (var_19: int64), (var_20: int64), (var_21: int64), (var_22: int64), (var_23: int64), (var_24: int32), (var_25: float32), (var_26: (Union0 ref)), (var_27: int64), (var_28: int64), (var_29: int64), (var_30: int64), (var_31: int64), (var_32: int64), (var_33: int64), (var_34: int64), (var_35: int32)): unit =
    let (var_36: ManagedCuda.CudaBlas.Operation) = ManagedCuda.CudaBlas.Operation.Transpose
    let (var_37: ManagedCuda.CudaBlas.Operation) = ManagedCuda.CudaBlas.Operation.NonTranspose
    let (var_38: (float32 ref)) = (ref var_4)
    let (var_39: bool) = (var_10 < var_11)
    let (var_40: bool) = (var_39 = false)
    if var_40 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_41: bool) = (var_12 < var_13)
    let (var_42: bool) = (var_41 = false)
    if var_42 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_43: int64) = (var_13 - var_12)
    let (var_44: int64) = (var_11 - var_10)
    let (var_45: int64) = (var_44 * var_43)
    let (var_46: bool) = (0L = var_7)
    let (var_47: bool) = (var_46 = false)
    if var_47 then
        (failwith "The inner dimensions much have offsets of 0. They must not be 'view'ed. Consider reshaping a copy of the tensor instead")
    else
        ()
    let (var_48: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_5: (Union0 ref)))
    let (var_49: ManagedCuda.BasicTypes.SizeT) = var_48.Pointer
    let (var_50: uint64) = uint64 var_49
    let (var_51: uint64) = (uint64 var_6)
    let (var_52: uint64) = (var_50 + var_51)
    let (var_53: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_52)
    let (var_54: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_53)
    let (var_55: bool) = (var_20 < var_21)
    let (var_56: bool) = (var_55 = false)
    if var_56 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_57: bool) = (var_22 < var_23)
    let (var_58: bool) = (var_57 = false)
    if var_58 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_59: int64) = (var_23 - var_22)
    let (var_60: int64) = (var_21 - var_20)
    let (var_61: int64) = (var_60 * var_59)
    let (var_62: bool) = (0L = var_17)
    let (var_63: bool) = (var_62 = false)
    if var_63 then
        (failwith "The inner dimensions much have offsets of 0. They must not be 'view'ed. Consider reshaping a copy of the tensor instead")
    else
        ()
    let (var_64: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_15: (Union0 ref)))
    let (var_65: ManagedCuda.BasicTypes.SizeT) = var_64.Pointer
    let (var_66: uint64) = uint64 var_65
    let (var_67: uint64) = (uint64 var_16)
    let (var_68: uint64) = (var_66 + var_67)
    let (var_69: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_68)
    let (var_70: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_69)
    let (var_71: (float32 ref)) = (ref var_25)
    let (var_72: bool) = (var_31 < var_32)
    let (var_73: bool) = (var_72 = false)
    if var_73 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_74: bool) = (var_33 < var_34)
    let (var_75: bool) = (var_74 = false)
    if var_75 then
        (failwith "Tensor needs to be at least size 1.")
    else
        ()
    let (var_76: int64) = (var_34 - var_33)
    let (var_77: int64) = (var_32 - var_31)
    let (var_78: int64) = (var_77 * var_76)
    let (var_79: bool) = (0L = var_28)
    let (var_80: bool) = (var_79 = false)
    if var_80 then
        (failwith "The inner dimensions much have offsets of 0. They must not be 'view'ed. Consider reshaping a copy of the tensor instead")
    else
        ()
    let (var_81: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_26: (Union0 ref)))
    let (var_82: ManagedCuda.BasicTypes.SizeT) = var_81.Pointer
    let (var_83: uint64) = uint64 var_82
    let (var_84: uint64) = (uint64 var_27)
    let (var_85: uint64) = (var_83 + var_84)
    let (var_86: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_85)
    let (var_87: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_86)
    let (var_88: ManagedCuda.CudaBlas.CublasStatus) = ManagedCuda.CudaBlas.CudaBlasNativeMethods.cublasSgemm_v2(var_0, var_36, var_37, var_1, var_2, var_3, var_38, var_54, var_14, var_70, var_24, var_71, var_87, var_35)
    if var_88 <> ManagedCuda.CudaBlas.CublasStatus.Success then raise <| new ManagedCuda.CudaBlas.CudaBlasException(var_88)
and method_3((var_0: ManagedCuda.BasicTypes.CUdeviceptr), (var_1: uint64), (var_2: uint64), (var_3: int64), (var_4: System.Collections.Generic.Stack<Env2>), (var_5: Env3), (var_6: int64)): Env3 =
    let (var_7: ManagedCuda.BasicTypes.SizeT) = var_0.Pointer
    let (var_8: uint64) = uint64 var_7
    let (var_9: uint64) = uint64 var_6
    let (var_10: uint64) = (var_8 - var_1)
    let (var_11: uint64) = (var_10 + var_9)
    let (var_12: uint64) = uint64 var_3
    let (var_13: uint64) = (var_12 + var_11)
    let (var_14: bool) = (var_13 <= var_2)
    let (var_15: bool) = (var_14 = false)
    if var_15 then
        (failwith "Cache size has been exceeded in the allocator.")
    else
        ()
    let (var_16: uint64) = (var_8 + var_9)
    let (var_17: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_16)
    let (var_18: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_17)
    let (var_19: (Union0 ref)) = (ref (Union0Case0(Tuple1(var_18))))
    var_4.Push((Env2((Env3(var_19)), var_3)))
    (Env3(var_19))
and method_4((var_0: uint64), (var_1: uint64), (var_2: int64), (var_3: System.Collections.Generic.Stack<Env2>)): Env3 =
    let (var_4: uint64) = uint64 var_2
    let (var_5: bool) = (var_4 <= var_1)
    let (var_6: bool) = (var_5 = false)
    if var_6 then
        (failwith "Cache size has been exceeded in the allocator.")
    else
        ()
    let (var_7: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_0)
    let (var_8: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_7)
    let (var_9: (Union0 ref)) = (ref (Union0Case0(Tuple1(var_8))))
    var_3.Push((Env2((Env3(var_9)), var_2)))
    (Env3(var_9))
and method_14((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_2: (Union4 ref))): float32 =
    let (var_3: float32) = method_15((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_2: (Union4 ref)))
    (var_3 / 2.000000f)
and method_12((var_0: (Union0 ref)), (var_1: ManagedCuda.CudaContext)): float32 =
    let (var_2: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_0: (Union0 ref)))
    let (var_3: (float32 [])) = Array.zeroCreate<float32> (System.Convert.ToInt32(1L))
    var_1.CopyToHost(var_3, var_2)
    var_1.Synchronize()
    let (var_4: float32) = 0.000000f
    let (var_5: int64) = 0L
    method_13((var_3: (float32 [])), (var_4: float32), (var_5: int64))
and method_13((var_0: (float32 [])), (var_1: float32), (var_2: int64)): float32 =
    let (var_3: bool) = (var_2 < 1L)
    if var_3 then
        let (var_4: bool) = (var_2 >= 0L)
        let (var_5: bool) = (var_4 = false)
        if var_5 then
            (failwith "Argument out of bounds.")
        else
            ()
        let (var_6: float32) = var_0.[int32 var_2]
        let (var_7: float32) = (var_1 + var_6)
        let (var_8: int64) = (var_2 + 1L)
        method_13((var_0: (float32 [])), (var_7: float32), (var_8: int64))
    else
        var_1
let (var_0: string) = cuda_kernels
let (var_1: ManagedCuda.CudaContext) = ManagedCuda.CudaContext(false)
var_1.Synchronize()
let (var_2: string) = System.Environment.get_CurrentDirectory()
let (var_3: string) = System.IO.Path.Combine(var_2, "nvcc_router.bat")
let (var_4: System.Diagnostics.ProcessStartInfo) = System.Diagnostics.ProcessStartInfo()
var_4.set_RedirectStandardOutput(true)
var_4.set_RedirectStandardError(true)
var_4.set_UseShellExecute(false)
var_4.set_FileName(var_3)
let (var_5: System.Diagnostics.Process) = System.Diagnostics.Process()
var_5.set_StartInfo(var_4)
let (var_7: (System.Diagnostics.DataReceivedEventArgs -> unit)) = method_0
var_5.OutputDataReceived.Add(var_7)
var_5.ErrorDataReceived.Add(var_7)
let (var_8: string) = System.IO.Path.Combine("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community", "VC\\Auxiliary\\Build\\vcvars64.bat")
let (var_9: string) = System.IO.Path.Combine("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community", "VC\\Tools\\MSVC\\14.11.25503\\bin\\Hostx64\\x64")
let (var_10: string) = System.IO.Path.Combine("C:\\Program Files\\NVIDIA GPU Computing Toolkit\\CUDA\\v9.0", "include")
let (var_11: string) = System.IO.Path.Combine("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community", "VC\\Tools\\MSVC\\14.11.25503\\include")
let (var_12: string) = System.IO.Path.Combine("C:\\Program Files\\NVIDIA GPU Computing Toolkit\\CUDA\\v9.0", "bin\\nvcc.exe")
let (var_13: string) = System.IO.Path.Combine(var_2, "cuda_kernels.ptx")
let (var_14: string) = System.IO.Path.Combine(var_2, "cuda_kernels.cu")
let (var_15: bool) = System.IO.File.Exists(var_14)
if var_15 then
    System.IO.File.Delete(var_14)
else
    ()
System.IO.File.WriteAllText(var_14, var_0)
let (var_16: bool) = System.IO.File.Exists(var_3)
if var_16 then
    System.IO.File.Delete(var_3)
else
    ()
let (var_17: System.IO.FileStream) = System.IO.File.OpenWrite(var_3)
let (var_18: System.IO.StreamWriter) = System.IO.StreamWriter(var_17)
var_18.WriteLine("SETLOCAL")
let (var_19: string) = String.concat "" [|"CALL "; "\""; var_8; "\""|]
var_18.WriteLine(var_19)
let (var_20: string) = String.concat "" [|"SET PATH=%PATH%;"; "\""; var_9; "\""|]
var_18.WriteLine(var_20)
let (var_21: string) = String.concat "" [|"\""; var_12; "\" -gencode=arch=compute_30,code=\\\"sm_30,compute_30\\\" --use-local-env --cl-version 2017 -I\""; var_10; "\" -I\"C:\\cub-1.7.4\" -I\""; var_11; "\" --keep-dir \""; var_2; "\" -maxrregcount=0  --machine 64 -ptx -cudart static  -o \""; var_13; "\" \""; var_14; "\""|]
var_18.WriteLine(var_21)
var_18.Dispose()
var_17.Dispose()
let (var_22: System.Diagnostics.Stopwatch) = System.Diagnostics.Stopwatch.StartNew()
let (var_23: bool) = var_5.Start()
let (var_24: bool) = (var_23 = false)
if var_24 then
    (failwith "NVCC failed to run.")
else
    ()
var_5.BeginOutputReadLine()
var_5.BeginErrorReadLine()
var_5.WaitForExit()
let (var_25: int32) = var_5.get_ExitCode()
let (var_26: bool) = (var_25 = 0)
let (var_27: bool) = (var_26 = false)
if var_27 then
    let (var_28: string) = System.String.Format("{0}",var_25)
    let (var_29: string) = String.concat ", " [|"NVCC failed compilation."; var_28|]
    let (var_30: string) = System.String.Format("[{0}]",var_29)
    (failwith var_30)
else
    ()
let (var_31: System.TimeSpan) = var_22.get_Elapsed()
printfn "The time it took to compile the Cuda kernels is: %A" var_31
let (var_32: ManagedCuda.BasicTypes.CUmodule) = var_1.LoadModulePTX(var_13)
var_5.Dispose()
let (var_33: string) = String.concat "" [|"Compiled the kernels into the following directory: "; var_2|]
let (var_34: string) = System.String.Format("{0}",var_33)
System.Console.WriteLine(var_34)
let (var_35: ManagedCuda.CudaDeviceProperties) = var_1.GetDeviceInfo()
let (var_36: ManagedCuda.BasicTypes.SizeT) = var_35.get_TotalGlobalMemory()
let (var_37: int64) = int64 var_36
let (var_38: float) = float var_37
let (var_39: float) = (0.700000 * var_38)
let (var_40: int64) = int64 var_39
let (var_41: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_40)
let (var_42: ManagedCuda.BasicTypes.CUdeviceptr) = var_1.AllocateMemory(var_41)
let (var_43: (Union0 ref)) = (ref (Union0Case0(Tuple1(var_42))))
let (var_44: System.Collections.Generic.Stack<Env2>) = System.Collections.Generic.Stack<Env2>()
let (var_45: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_43: (Union0 ref)))
let (var_46: ManagedCuda.BasicTypes.SizeT) = var_45.Pointer
let (var_47: uint64) = uint64 var_46
let (var_48: uint64) = uint64 var_40
let (var_49: ManagedCuda.CudaStream) = ManagedCuda.CudaStream()
let (var_50: ManagedCuda.CudaRand.GeneratorType) = ManagedCuda.CudaRand.GeneratorType.PseudoDefault
let (var_51: ManagedCuda.CudaRand.CudaRandDevice) = ManagedCuda.CudaRand.CudaRandDevice(var_50)
let (var_52: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
var_51.SetStream(var_52)
let (var_53: ManagedCuda.CudaBlas.PointerMode) = ManagedCuda.CudaBlas.PointerMode.Host
let (var_54: ManagedCuda.CudaBlas.AtomicsMode) = ManagedCuda.CudaBlas.AtomicsMode.Allowed
let (var_55: ManagedCuda.CudaBlas.CudaBlas) = ManagedCuda.CudaBlas.CudaBlas(var_53, var_54)
let (var_56: ManagedCuda.CudaBlas.CudaBlasHandle) = var_55.get_CublasHandle()
let (var_57: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
var_55.set_Stream(var_57)
let (var_58: int64) = 48L
let (var_59: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_58: int64))
let (var_60: (Union0 ref)) = var_59.mem_0
let (var_61: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_60: (Union0 ref)))
let (var_62: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(12L)
var_51.GenerateNormal32(var_61, var_62, 0.000000f, 1.000000f)
let (var_63: int64) = 32L
let (var_64: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_63: int64))
let (var_65: (Union0 ref)) = var_64.mem_0
let (var_66: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_65: (Union0 ref)))
let (var_67: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
let (var_68: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(32L)
var_1.ClearMemoryAsync(var_66, 0uy, var_68, var_67)
let (var_69: int64) = 96L
let (var_70: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_69: int64))
let (var_71: (Union0 ref)) = var_70.mem_0
let (var_72: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_71: (Union0 ref)))
let (var_73: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(24L)
var_51.GenerateNormal32(var_72, var_73, 0.000000f, 0.447214f)
let (var_74: int64) = 96L
let (var_75: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_74: int64))
let (var_76: (Union0 ref)) = var_75.mem_0
let (var_77: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_76: (Union0 ref)))
let (var_78: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
let (var_79: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(96L)
var_1.ClearMemoryAsync(var_77, 0uy, var_79, var_78)
let (var_80: int64) = 32L
let (var_81: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_80: int64))
let (var_82: (Union0 ref)) = var_81.mem_0
let (var_83: int32) = 2
let (var_84: int32) = 4
let (var_85: int32) = 6
let (var_86: float32) = 1.000000f
let (var_87: int64) = 0L
let (var_88: int64) = 0L
let (var_89: int64) = 6L
let (var_90: int64) = 1L
let (var_91: int64) = 0L
let (var_92: int64) = 2L
let (var_93: int64) = 0L
let (var_94: int64) = 6L
let (var_95: int32) = 2
let (var_96: int64) = 0L
let (var_97: int64) = 0L
let (var_98: int64) = 4L
let (var_99: int64) = 1L
let (var_100: int64) = 0L
let (var_101: int64) = 6L
let (var_102: int64) = 0L
let (var_103: int64) = 4L
let (var_104: int32) = 6
let (var_105: float32) = 0.000000f
let (var_106: int64) = 0L
let (var_107: int64) = 0L
let (var_108: int64) = 4L
let (var_109: int64) = 1L
let (var_110: int64) = 0L
let (var_111: int64) = 2L
let (var_112: int64) = 0L
let (var_113: int64) = 4L
let (var_114: int32) = 2
method_6((var_56: ManagedCuda.CudaBlas.CudaBlasHandle), (var_83: int32), (var_84: int32), (var_85: int32), (var_86: float32), (var_60: (Union0 ref)), (var_87: int64), (var_88: int64), (var_89: int64), (var_90: int64), (var_91: int64), (var_92: int64), (var_93: int64), (var_94: int64), (var_95: int32), (var_71: (Union0 ref)), (var_96: int64), (var_97: int64), (var_98: int64), (var_99: int64), (var_100: int64), (var_101: int64), (var_102: int64), (var_103: int64), (var_104: int32), (var_105: float32), (var_82: (Union0 ref)), (var_106: int64), (var_107: int64), (var_108: int64), (var_109: int64), (var_110: int64), (var_111: int64), (var_112: int64), (var_113: int64), (var_114: int32))
let (var_115: int64) = 32L
let (var_116: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_115: int64))
let (var_117: (Union0 ref)) = var_116.mem_0
let (var_118: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_117: (Union0 ref)))
let (var_119: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
let (var_120: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(32L)
var_1.ClearMemoryAsync(var_118, 0uy, var_120, var_119)
let (var_125: int64) = 32L
let (var_126: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_125: int64))
let (var_127: (Union0 ref)) = var_126.mem_0
let (var_128: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_82: (Union0 ref)))
let (var_129: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_127: (Union0 ref)))
// Cuda join point
// method_7((var_128: ManagedCuda.BasicTypes.CUdeviceptr), (var_129: ManagedCuda.BasicTypes.CUdeviceptr))
let (var_131: (System.Object [])) = [|var_128; var_129|]: (System.Object [])
let (var_132: ManagedCuda.CudaKernel) = ManagedCuda.CudaKernel("method_7", var_32, var_1)
let (var_133: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(1u, 1u, 1u)
var_132.set_GridDimensions(var_133)
let (var_134: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(128u, 1u, 1u)
var_132.set_BlockDimensions(var_134)
let (var_135: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
var_132.RunAsync(var_135, var_131)
let (var_136: int64) = 32L
let (var_137: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_136: int64))
let (var_138: (Union0 ref)) = var_137.mem_0
let (var_139: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_138: (Union0 ref)))
let (var_140: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
let (var_141: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(32L)
var_1.ClearMemoryAsync(var_139, 0uy, var_141, var_140)
let (var_142: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_127: (Union0 ref)))
let (var_143: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_65: (Union0 ref)))
let (var_146: int64) = 4L
let (var_147: Env3) = method_2((var_47: uint64), (var_44: System.Collections.Generic.Stack<Env2>), (var_48: uint64), (var_146: int64))
let (var_148: (Union0 ref)) = var_147.mem_0
let (var_149: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_148: (Union0 ref)))
// Cuda join point
// method_9((var_142: ManagedCuda.BasicTypes.CUdeviceptr), (var_143: ManagedCuda.BasicTypes.CUdeviceptr), (var_149: ManagedCuda.BasicTypes.CUdeviceptr))
let (var_151: (System.Object [])) = [|var_142; var_143; var_149|]: (System.Object [])
let (var_152: ManagedCuda.CudaKernel) = ManagedCuda.CudaKernel("method_9", var_32, var_1)
let (var_153: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(1u, 1u, 1u)
var_152.set_GridDimensions(var_153)
let (var_154: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(128u, 1u, 1u)
var_152.set_BlockDimensions(var_154)
let (var_155: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
var_152.RunAsync(var_155, var_151)
let (var_157: (Union4 ref)) = (ref Union4Case1)
let (var_158: (float32 ref)) = (ref 0.000000f)
let (var_160: (Union4 ref)) = (ref Union4Case1)
let (var_161: (float32 ref)) = (ref 0.000000f)
let (var_162: float32) = method_16((var_148: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_157: (Union4 ref)), (var_160: (Union4 ref)))
let (var_163: string) = System.String.Format("{0}",var_162)
let (var_164: string) = String.concat ", " [|"Cost is:"; var_163|]
let (var_165: string) = System.String.Format("[{0}]",var_164)
System.Console.WriteLine(var_165)
var_161 := 1.000000f
let (var_166: float32) = method_16((var_148: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_157: (Union4 ref)), (var_160: (Union4 ref)))
let (var_167: float32) = (!var_161)
let (var_168: float32) = method_15((var_148: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_157: (Union4 ref)))
let (var_169: float32) = (var_167 / 2.000000f)
let (var_170: float32) = (!var_158)
let (var_171: float32) = (var_170 + var_169)
var_158 := var_171
let (var_172: float32) = method_15((var_148: (Union0 ref)), (var_1: ManagedCuda.CudaContext), (var_157: (Union4 ref)))
let (var_173: float32) = (!var_158)
let (var_174: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_138: (Union0 ref)))
let (var_175: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_127: (Union0 ref)))
let (var_176: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_65: (Union0 ref)))
let (var_177: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_138: (Union0 ref)))
// Cuda join point
// method_17((var_173: float32), (var_172: float32), (var_174: ManagedCuda.BasicTypes.CUdeviceptr), (var_175: ManagedCuda.BasicTypes.CUdeviceptr), (var_176: ManagedCuda.BasicTypes.CUdeviceptr), (var_177: ManagedCuda.BasicTypes.CUdeviceptr))
let (var_179: (System.Object [])) = [|var_173; var_172; var_174; var_175; var_176; var_177|]: (System.Object [])
let (var_180: ManagedCuda.CudaKernel) = ManagedCuda.CudaKernel("method_17", var_32, var_1)
let (var_181: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(1u, 1u, 1u)
var_180.set_GridDimensions(var_181)
let (var_182: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(128u, 1u, 1u)
var_180.set_BlockDimensions(var_182)
let (var_183: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
var_180.RunAsync(var_183, var_179)
let (var_184: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_117: (Union0 ref)))
let (var_185: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_82: (Union0 ref)))
let (var_186: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_138: (Union0 ref)))
let (var_187: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_127: (Union0 ref)))
let (var_188: ManagedCuda.BasicTypes.CUdeviceptr) = method_5((var_117: (Union0 ref)))
// Cuda join point
// method_19((var_184: ManagedCuda.BasicTypes.CUdeviceptr), (var_185: ManagedCuda.BasicTypes.CUdeviceptr), (var_186: ManagedCuda.BasicTypes.CUdeviceptr), (var_187: ManagedCuda.BasicTypes.CUdeviceptr), (var_188: ManagedCuda.BasicTypes.CUdeviceptr))
let (var_190: (System.Object [])) = [|var_184; var_185; var_186; var_187; var_188|]: (System.Object [])
let (var_191: ManagedCuda.CudaKernel) = ManagedCuda.CudaKernel("method_19", var_32, var_1)
let (var_192: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(1u, 1u, 1u)
var_191.set_GridDimensions(var_192)
let (var_193: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(128u, 1u, 1u)
var_191.set_BlockDimensions(var_193)
let (var_194: ManagedCuda.BasicTypes.CUstream) = var_49.get_Stream()
var_191.RunAsync(var_194, var_190)
let (var_195: int32) = 6
let (var_196: int32) = 4
let (var_197: int32) = 2
let (var_198: float32) = 1.000000f
let (var_199: int64) = 0L
let (var_200: int64) = 0L
let (var_201: int64) = 6L
let (var_202: int64) = 1L
let (var_203: int64) = 0L
let (var_204: int64) = 2L
let (var_205: int64) = 0L
let (var_206: int64) = 6L
let (var_207: int32) = 2
let (var_208: int64) = 0L
let (var_209: int64) = 0L
let (var_210: int64) = 4L
let (var_211: int64) = 1L
let (var_212: int64) = 0L
let (var_213: int64) = 2L
let (var_214: int64) = 0L
let (var_215: int64) = 4L
let (var_216: int32) = 2
let (var_217: float32) = 1.000000f
let (var_218: int64) = 0L
let (var_219: int64) = 0L
let (var_220: int64) = 4L
let (var_221: int64) = 1L
let (var_222: int64) = 0L
let (var_223: int64) = 6L
let (var_224: int64) = 0L
let (var_225: int64) = 4L
let (var_226: int32) = 6
method_21((var_56: ManagedCuda.CudaBlas.CudaBlasHandle), (var_195: int32), (var_196: int32), (var_197: int32), (var_198: float32), (var_60: (Union0 ref)), (var_199: int64), (var_200: int64), (var_201: int64), (var_202: int64), (var_203: int64), (var_204: int64), (var_205: int64), (var_206: int64), (var_207: int32), (var_117: (Union0 ref)), (var_208: int64), (var_209: int64), (var_210: int64), (var_211: int64), (var_212: int64), (var_213: int64), (var_214: int64), (var_215: int64), (var_216: int32), (var_217: float32), (var_76: (Union0 ref)), (var_218: int64), (var_219: int64), (var_220: int64), (var_221: int64), (var_222: int64), (var_223: int64), (var_224: int64), (var_225: int64), (var_226: int32))
var_148 := Union0Case1
var_138 := Union0Case1
var_127 := Union0Case1
var_117 := Union0Case1
var_82 := Union0Case1
var_76 := Union0Case1
var_71 := Union0Case1
var_65 := Union0Case1
var_60 := Union0Case1
var_55.Dispose()
var_51.Dispose()
var_49.Dispose()
let (var_227: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_43: (Union0 ref)))
var_1.FreeMemory(var_227)
var_43 := Union0Case1
var_1.Dispose()

