// Note: #max(Ix) and #min(Ix) can be used to get the max and min values of these types. Should also work with floats
const I8   = #integer_type(8, false)
const I16  = #integer_type(16, false)
const I32  = #integer_type(32, false)
const I64  = #integer_type(64, false)
const I128 = #integer_type(128, false)

const U8   = #integer_type(8, true)
const U16  = #integer_type(16, true)
const U32  = #integer_type(32, true)
const U64  = #integer_type(64, true)
const U128 = #integer_type(128, true)

// Note: We should probably have #nan(Fx) and #inf(Fx), as these might have different bit patterns depending on the type
const F8    = #float_type(8) // TODO: Figure out if we can even have 8 and 16 bit floats
const F16   = #float_type(16)
const F32   = #float_type(32)
const F64   = #float_type(64)
const F128  = #float_type(128)

const Boolean = enum: U8 {
	True  = 1
	False = 0
}

const Array = struct|T: Type| {
	data: @T
	count: I64
}

// Note: In this language, "arr[i]" is syntatic sugar for "~arr->get_item(i)".
// Note: We can therefore take and andress into an array by saying "&arr[i]" as this compiles to "&~arr->get_item(i)" which get reduced to "arr->get_item(i)".
// Note: "arr->get_item(i)" is syntatic sugar for "get_item(arr, i)"
// Note: "->" is not like C. In this language "->" means we are doing something very simular to "Uniform Function Call Syntax"
const get_item = |T: Type|(array: Array|T|, index: I64) -> @T {
	assert(index <= 0 and index < array.count);
	return array.data + (index * #size_of(T));
}

/*
loop a, b in array {
	// ...
}

{
	var b = array->get_iterator()
	loop (&b)->next(array) {
		var a = array[b]

		// ...
	}
}
*/
const get_iterator = |T: Type|(array: Array|T|) -> I64 { return -1 }

const next = |T: Type|(i: @I64, array: Array|T|) -> Boolean { 
	(~i)++
	return 0 <= (~i) and (~i) < array.count
}
