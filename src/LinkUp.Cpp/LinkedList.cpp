#include "LinkedList.h"

LinkedList::LinkedList()
{
	pRoot = NULL;
}

void LinkedList::insert(void* pData) {
	LinkedListElement* pNew = (LinkedListElement*)calloc(1, sizeof(LinkedListElement));
	pNew->pData = pData;
	pNew->pNext = pRoot;
	pRoot = pNew;
}

void LinkedList::remove(void* pData) {
	if (pData != NULL) {
		LinkedListElement* pCurrent = pRoot;
		LinkedListElement* pPrev = pRoot;

		while (pCurrent != NULL) {
			if (pCurrent->pData == pData)
			{
				if (pCurrent == pRoot)
				{
					pRoot = pCurrent->pNext;
				}
				else
				{
					pPrev->pNext = pCurrent->pNext;
				}
				free(pCurrent);
				pCurrent = 0;
			}
			else
			{
				pPrev = pCurrent;
				pCurrent = pCurrent->pNext;
			}
		}
	}
}

LinkedList* LinkedList::clone() {
	LinkedList * pClone = new LinkedList();

	LinkedListElement* pCurrent = pRoot;

	while (pCurrent != NULL) {
		pClone->insert(pCurrent->pData);
		pCurrent = pCurrent->pNext;
	}
	return pClone;
}

void* LinkedListIterator::next() {
	void* pData = NULL;
	LinkedListElement* pTemp = NULL;

	if (pNext != NULL)
	{
		pData = pNext->pData;
		pTemp = pNext;
		pNext = pTemp->pNext;
		free(pTemp);
	}
	return pData;
}

LinkedListIterator::LinkedListIterator(LinkedList* pList) {
	LinkedList* pClone = pList->clone();
	pNext = pClone->pRoot;
	free(pClone);
}