#ifndef _LINKED_LIST_h
#define _LINKED_LIST_h

#include "Platform.h"

struct LinkedListElement
{
	void *pData;
	LinkedListElement *pNext;
};

class LinkedList {
public:
	LinkedListElement * pRoot;
	LinkedList();
	void insert(void* pData);
	void remove(void* pData);
	LinkedList* clone();
};

class LinkedListIterator
{
	LinkedListElement *pNext;
public:
	void* next();
	LinkedListIterator(LinkedList* pList);
};

#endif