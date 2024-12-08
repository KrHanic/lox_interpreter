/*
* Next, we need a module to define our code representation. I’ve been using “chunk” to refer to sequences of bytecode, so let’s make that the official name for that module.
*/

#ifndef clox_chunk_h
#define clox_chunk_h

#include "common.h"

typedef enum {
	OP_RETURN, // return from the current function
} OpCode;



#endif
