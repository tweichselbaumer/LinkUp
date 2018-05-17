#include "AVLTree.h"

using namespace std;

void AvlTree::clear(AvlNode* pNode) {
	if (pNode == NULL)
		return;
	clear(pNode->pLeft);
	clear(pNode->pRight);
	delete pNode;
}

AvlNode* AvlTree::insert(uint16_t nId, void* pData, AvlNode* pNode) {
	if (pNode == NULL)
	{
		pNode = (AvlNode *)calloc(1, sizeof(AvlNode));
		pNode->nId = nId;
		pNode->pData = pData;
		pNode->pLeft = pNode->pRight = NULL;
	}
	else if (nId < pNode->nId)
	{
		pNode->pLeft = insert(nId, pData, pNode->pLeft);
		if (height(pNode->pLeft) - height(pNode->pRight) == 2)
		{
			if (nId < pNode->pLeft->nId)
				pNode = singleRightRotate(pNode);
			else
				pNode = doubleRightRotate(pNode);
		}
	}
	else if (nId > pNode->nId)
	{
		pNode->pRight = insert(nId, pData, pNode->pRight);
		if (height(pNode->pRight) - height(pNode->pLeft) == 2)
		{
			if (nId > pNode->pRight->nId)
				pNode = singleLeftRotate(pNode);
			else
				pNode = doubleLeftRotate(pNode);
		}
	}
	return pNode;
}

AvlNode* AvlTree::singleRightRotate(AvlNode* &pNode) {
	AvlNode* pU = pNode->pLeft;
	pNode->pLeft = pU->pRight;
	pU->pRight = pNode;
	return pU;
}

AvlNode* AvlTree::singleLeftRotate(AvlNode* &pNode) {
	AvlNode* pU = pNode->pRight;
	pNode->pRight = pU->pLeft;
	pU->pLeft = pNode;
	return pU;
}

AvlNode* AvlTree::doubleLeftRotate(AvlNode* &pNode) {
	pNode->pRight = singleRightRotate(pNode->pRight);
	return singleLeftRotate(pNode);
}

AvlNode* AvlTree::doubleRightRotate(AvlNode* &pNode) {
	pNode->pLeft = singleLeftRotate(pNode->pLeft);
	return singleRightRotate(pNode);
}

AvlNode* AvlTree::findMin(AvlNode* pNode) {
	if (pNode == NULL)
		return NULL;
	else if (pNode->pLeft == NULL)
		return pNode;
	else
		return findMin(pNode->pLeft);
}

AvlNode* AvlTree::findMax(AvlNode* pNode) {
	if (pNode == NULL)
		return NULL;
	else if (pNode->pRight == NULL)
		return pNode;
	else
		return findMax(pNode->pRight);
}

AvlNode* AvlTree::find(uint16_t nId, AvlNode* pNode) {
	if (pNode == NULL)
		return NULL;
	else if (nId == pNode->nId)
		return pNode;
	else if (nId < pNode->nId)
		find(nId, pNode->pLeft);
	else if (nId > pNode->nId)
		find(nId, pNode->pRight);
}

AvlNode* AvlTree::remove(uint16_t nId, AvlNode* pNode) {
	AvlNode* pTemp;

	if (pNode == NULL)
		return NULL;
	else if (nId < pNode->nId)
		pNode->pLeft = remove(nId, pNode->pLeft);
	else if (nId > pNode->nId)
		pNode->pRight = remove(nId, pNode->pRight);

	// Element found
	// With 2 children
	else if (pNode->pLeft && pNode->pRight)
	{
		pTemp = findMin(pNode->pRight);
		pNode->nId = pTemp->nId;
		pNode->pRight = remove(pNode->nId, pNode->pRight);
	}
	// With one or zero child
	else
	{
		pTemp = pNode;
		if (pNode->pLeft == NULL)
			pNode = pNode->pRight;
		else if (pNode->pRight == NULL)
			pNode = pNode->pLeft;
		if (pTemp != NULL && pTemp->pData != NULL)
			free(pTemp->pData);
		delete pTemp;
	}
	if (pNode == NULL)
		return pNode;

	// If node is unbalanced
	// If left node is deleted, right case
	if (height(pNode->pLeft) - height(pNode->pRight) == 2)
	{
		// right right case
		if (height(pNode->pLeft->pLeft) - height(pNode->pLeft->pRight) == 1)
			return singleLeftRotate(pNode);
		// right left case
		else
			return doubleLeftRotate(pNode);
	}
	// If right node is deleted, left case
	else if (height(pNode->pRight) - height(pNode->pLeft) == 2)
	{
		// left left case
		if (height(pNode->pRight->pRight) - height(pNode->pRight->pLeft) == 1)
			return singleRightRotate(pNode);
		// left right case
		else
			return doubleRightRotate(pNode);
	}
	return pNode;
}

int32_t AvlTree::height(AvlNode* pNode) {
	return (pNode == NULL ? -1 : max(height(pNode->pLeft), height(pNode->pRight)) + 1);
}

uint32_t AvlTree::getBalance(AvlNode* pNode) {
	if (pNode == NULL)
		return 0;
	else
		return height(pNode->pLeft) - height(pNode->pRight);
}

AvlTree::AvlTree()
{
	pRoot = NULL;
}

void AvlTree::insert(uint16_t nId, void* pData)
{
	pRoot = insert(nId, pData, pRoot);
	//AvlTree::printDot();
}

void AvlTree::remove(uint16_t nId)
{
	pRoot = remove(nId, pRoot);
}

AvlNode* AvlTree::find(uint16_t nId) {
	return find(nId, pRoot);
}

AvlTreeList* AvlTree::getList(AvlNode* pNode, AvlTreeList* pLast) {
	if (pNode != NULL) {
		AvlTreeList* pLeftList = getList(pNode->pLeft, pLast);

		AvlTreeList *pNext = (AvlTreeList *)calloc(1, sizeof(AvlTreeList));
		pNext->pNode = pNode;

		if (pLeftList != NULL) {
			pLeftList->pNext = pNext;
		}

		return getList(pNode->pRight, pNext);
	}
	return pLast;
}

AvlTreeList* AvlTree::getList() {
	AvlTreeList first = { 0 };
	getList(pRoot, &first);
	return first.pNext;
}

AvlTreeIterator::AvlTreeIterator(AvlTree* pTree) {
	pNext = pTree->getList();
}

AvlNode* AvlTreeIterator::next() {
	AvlNode* pNode = NULL;
	AvlTreeList* pTemp = NULL;
	if (pNext != NULL)
	{
		pNode = pNext->pNode;
		pTemp = pNext;
		pNext = pTemp->pNext;
		free(pTemp);
	}
	return pNode;
}

void AvlTree::printDot() {
	ofstream file;
	char str[1024];
	sprintf(str, "C:\\Users\\thoma\\Desktop\\Dots\\tree%d.dot", dotPrintId++);
	file.open(str);
	file << "digraph BinaryTree {" << endl;
	printDot(pRoot, file);
	file << "}" << endl;
	file.close();
}

void AvlTree::printDot(AvlNode* pNode, std::ofstream& file)
{
	if (pNode != NULL)
	{
		file << pNode->nId << ";" << std::endl;
		if (pNode->pLeft != NULL) {
			file << pNode->nId << "->" << pNode->pLeft->nId << ";" << std::endl;
			printDot(pNode->pLeft, file);
		}
		else {
			file << "null" << pNode->nId << "l[shape=point];" << std::endl;
			file << pNode->nId << "-> null" << pNode->nId << "l;" << std::endl;
		}
		if (pNode->pRight != NULL) {
			file << pNode->nId << "->" << pNode->pRight->nId << ";" << std::endl;
			printDot(pNode->pRight, file);
		}
		else {
			file << "null" << pNode->nId << "r[shape=point];" << std::endl;
			file << pNode->nId << "-> null" << pNode->nId << "r;" << std::endl;
		}
	}
}