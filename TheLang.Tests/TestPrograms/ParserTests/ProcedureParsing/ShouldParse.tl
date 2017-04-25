procEmpty :: proc() Nothing => { }
funcEmpty :: func() Nothing => { }

procWithBody :: proc() Nothing => {
	test :: 1
}

funcWithBody :: func() Nothing => {
	test :: 1
}

procWithArgs :: proc(arg1: Int, arg2: Int) Int => { 
	return arg1 + arg2
}

funcWithArgs :: func(arg1: Int, arg2: Int) Int => { 
	return arg1 + arg2
}

procWithProcArg :: proc(arg1: proc(Int, Int) Int, arg2: Int, arg3: Int) Int => {
	return arg1(arg2, arg3)
}

funcWithFuncArg :: func(arg1: func(Int, Int) Int, arg2: Int, arg3: Int) Int => {
	return arg1(arg2, arg3)
}

main :: proc(args: []String) Nothing => {

}