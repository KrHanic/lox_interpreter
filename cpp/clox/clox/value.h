#ifndef clox_value_h
#define clox_value_h

#include "common.h"

/*
This typedef abstracts how Lox values are concretely represented in C. 
That way, we can change that representation without needing to go back and fix existing code that passes around values.
*/
typedef double Value;

// We use this to store constant values like literals.
typedef struct {
	int capacity;
	int count;
	Value* values;
} ValueArray;

void init_value_array(ValueArray* array);
void write_value_array(ValueArray* array, Value value);
void free_value_array(ValueArray* array);
void print_value(Value value);

#endif