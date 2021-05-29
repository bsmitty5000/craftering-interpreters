#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define MAX_STR_LEN 128

typedef struct StringNode
{
    struct StringNode* prev;
    struct StringNode* next;
    char* string;
} StringNode_t;

typedef struct StringList
{
    StringNode_t* head;
    StringNode_t* tail;
    int size;
}StringList_t;

// Inserts a string before insertBeforeMe
// Will intiialize an empty list if insertBeforeMe is null
// Will append to the end of the list if list is not empty and insertBeforeMe is null
int InsertBefore(StringList_t* l, StringNode_t* insertBeforeMe, char* stringToInsert);

// Deletes the given node
void Delete(StringList_t* l, StringNode_t* deleteMe);

StringNode_t* Find(StringList_t* l, char* stringToFind);

void PrintList(StringList_t* l);

int main()
{
    StringList_t myList = { 0 };

    InsertBefore(&myList, NULL, "First");
    InsertBefore(&myList, NULL, "Second");
    InsertBefore(&myList, NULL, "Third");
    InsertBefore(&myList, NULL, "Fourth");
    InsertBefore(&myList, NULL, "Fifth");

    PrintList(&myList);

    return 0;
}


int InsertBefore(StringList_t* l, StringNode_t* insertBeforeMe, char* stringToInsert)
{
    StringNode_t* newNode = (StringNode_t*)calloc(1u, sizeof(StringNode_t));
    if (newNode == NULL)
    {
        return -1;
    }

    newNode->string = (char*)calloc(strlen(stringToInsert) + 1, sizeof(char));
    if (newNode->string == NULL)
    {
        return -2;
    }

    strncpy(newNode->string, stringToInsert, strlen(stringToInsert));

    if ((insertBeforeMe == NULL) && (l->size == 0))
    {
        l->size = 1;
        l->head = newNode;
        l->tail = newNode;
    }
    else if (insertBeforeMe == NULL)
    {
        l->size++;
        l->tail->next = newNode;
        newNode->prev = l->tail;
        l->tail = newNode;
    }
    else
    { 
        l->size++;
        if (insertBeforeMe->prev == NULL)
        {
            l->head = newNode;
            newNode->next = insertBeforeMe;
            insertBeforeMe->prev = newNode;
        }
        else
        {
            newNode->next = insertBeforeMe;
            newNode->prev = insertBeforeMe->prev;
            insertBeforeMe->prev->next = newNode;
            insertBeforeMe->prev = newNode;
        }
    }

    return 0;
}

void Delete(StringList_t* l, StringNode_t* deleteMe)
{
    if (deleteMe == NULL)
    {
        return;
    }

    l->size--;

    if (deleteMe->next == NULL)
    {
        l->tail = deleteMe->prev;
    }
    else
    {
        deleteMe->next->prev = deleteMe->prev;
    }

    if (deleteMe->prev == NULL)
    {
        l->head = deleteMe->next;
    }
    else
    {
        deleteMe->prev->next = deleteMe->next;
    }
}

StringNode_t* Find(StringList_t* l, char* stringToFind)
{
    return NULL;
}

void PrintList(StringList_t* l)
{
    StringNode_t* nodeToPrint = l->head;

    while (nodeToPrint != NULL)
    {
        if (nodeToPrint->string != NULL)
        {
            printf("%s", nodeToPrint->string);

            if (nodeToPrint != l->tail)
            {
                printf(", ");
            }

            nodeToPrint = nodeToPrint->next;
        }
    }

    printf("\n");
}