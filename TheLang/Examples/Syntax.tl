test :: proc(x: Int, y: Int) Int => {
	return x + y
}

test :: proc(x: Int, y: Int) Int => x + y

test : proc(Int, Int) Int : (x, y) => {
	return x + y
}

test : proc(Int, Int) Int : (x, y) => x + y

test :: struct {
    test: Int

}