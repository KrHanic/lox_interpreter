#ifndef clox_memory_h
#define clox_memory_h

#include "common.h"

// @TODO: Maybe merge growing and realocate into one function?
#define GROW_CAPACITY(capacity) \
    ((capacity) < 8 ? 8 : (capacity) * 2)

#define GROW_ARRAY(type, pointer, old_capacity, new_capacity) \
    (type*)reallocate(pointer, sizeof(type) * (old_capacity), sizeof(type) * (new_capacity))

#define FREE_ARRAY(type, pointer, old_capacity) \
    reallocate(pointer, sizeof(type) * (old_capacity), 0)

void* reallocate(void* pointer, size_t oldSize, size_t newSize);

#endif