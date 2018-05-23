IF (WIN32)
#set(linkup_SOURCE_FILES1
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/CRC16.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/CRC16.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpNode.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpNode.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpRaw.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpRaw.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/Platform.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/Platform.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/AvlTree.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/AvlTree.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpLabel.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpLabel.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpLogic.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkedList.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkedList.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpPropertyLabel.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpPropertyLabel.cpp"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpEventLabel.h"
#  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpEventLabel.cpp"
#)
#
#file( COPY ${linkup_SOURCE_FILES1} DESTINATION "${PROJECT_SOURCE_DIR}/LinkUp/src" NO_SOURCE_PERMISSIONS )

set(linkup_SOURCE_FILES
"${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/CRC16.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/CRC16.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpNode.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpNode.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpRaw.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpRaw.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/Platform.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/Platform.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/AvlTree.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/AvlTree.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpLabel.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpLabel.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpLogic.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkedList.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkedList.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpPropertyLabel.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpPropertyLabel.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpEventLabel.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpEventLabel.cpp"
)

add_library(LinkUp ${linkup_SOURCE_FILES})
target_include_directories(LinkUp PUBLIC "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/")

find_package(Boost REQUIRED COMPONENTS system thread)
include_directories(${Boost_INCLUDE_DIR})
LINK_DIRECTORIES(${Boost_LIBRARY_DIRS})

ELSE()

#set(linkup_SOURCE_FILES
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/CRC16.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/CRC16.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpNode.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpNode.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpRaw.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpRaw.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/Platform.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/Platform.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/AvlTree.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/AvlTree.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpLabel.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpLabel.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpLogic.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkedList.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkedList.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpPropertyLabel.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpPropertyLabel.cpp"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpEventLabel.h"
#  "${PROJECT_SOURCE_DIR}/LinkUp/src/LinkUpEventLabel.cpp"
#)
#
#add_library(LinkUp ${linkup_SOURCE_FILES})
#target_include_directories(LinkUp PUBLIC "${PROJECT_SOURCE_DIR}/LinkUp/src")
#
#find_package(Boost REQUIRED COMPONENTS system thread)
#include_directories(${Boost_INCLUDE_DIR})
#LINK_DIRECTORIES(${Boost_LIBRARY_DIRS})

ENDIF()