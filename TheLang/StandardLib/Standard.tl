Nothing :: #type(#int, 0)

Int8  :: #type(#int, 8)
Int16 :: #type(#int, 16)
Int32 :: #type(#int, 32)
Int64 :: #type(#int, 64)
Int   :: Int64

UInt8  :: #type(#uint, 8)
UInt16 :: #type(#uint, 16)
UInt32 :: #type(#uint, 32)
UInt64 :: #type(#uint, 64)
UInt   :: UInt64

Float32  :: #type(#float, 32)
Float64  :: #type(#float, 64)
Float    :: Float64

Bool1 :: #type(#bool, 1)
Bool8 :: #type(#bool, 8)
Bool  :: Bool8

Array :: struct {
	capacity : Int
	items    : @UInt8
}

String :: struct {
	capacity : Int
	count    : Int
	items    : @UInt8
}

