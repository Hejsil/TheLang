#using(#load("Types.tl"));
#using(#load("Build.tl"))


const assert = (cond: Boolean, message: String = "", location: Location = #location()) {
	if #build_options() == Build_Option.Debug {
		if !cond {
			#panic("TODO: Location and message should be shown here")
		}
	}
}

const static_assert = |Cond: Boolean, Mes: String = "", Loc: Location = #location()| () {
	if !Cond {
		#compiler_error("TODO: Location and message should be shown here")
	}
}