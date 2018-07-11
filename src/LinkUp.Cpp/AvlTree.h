#ifndef _AVL_TREE_h
#define _AVL_TREE_h

#include "Platform.h"

using namespace std;

struct AvlNode
{
	uint16_t nId;
	void * pData;
	AvlNode* pLeft;
	AvlNode* pRight;
};

struct AvlTreeList
{
	AvlNode *pNode;
	AvlTreeList *pNext;
};

class AvlTree
{
private:
	AvlNode * pRoot;
	void clear(AvlNode* t);
	AvlNode* insert(uint16_t nId, void* pData, AvlNode* pNode);
	AvlNode* singleRightRotate(AvlNode* &pNode);
	AvlNode* singleLeftRotate(AvlNode* &pNode);
	AvlNode* doubleLeftRotate(AvlNode* &pNode);
	AvlNode* doubleRightRotate(AvlNode* &pNode);
	AvlNode* findMin(AvlNode* pNode);
	AvlNode* findMax(AvlNode* pNode);
	AvlNode* remove(uint16_t nId, AvlNode* pNode);
	int32_t height(AvlNode* pNode);
	AvlNode* find(uint16_t nId, AvlNode* pNode);
	AvlTreeList* getList(AvlNode* pNode, AvlTreeList* pLast);
#ifdef _WINDOWS
	int dotPrintId = 0;
	void printDot();
	void printDot(AvlNode* pNode, std::ofstream& file);
#endif
public:
	AvlTree();
	void insert(uint16_t nId, void* pData);
	void remove(uint16_t nId);
	uint32_t getBalance(AvlNode* pNode);
	AvlNode* find(uint16_t nId);
	AvlTreeList * getList();
};

class AvlTreeIterator
{
	AvlTreeList *pNext;
public:
	AvlNode * next();
	AvlTreeIterator(AvlTree* pTree);
};

#endif