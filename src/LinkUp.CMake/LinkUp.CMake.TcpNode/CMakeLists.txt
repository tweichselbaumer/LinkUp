cmake_minimum_required (VERSION 3.1)
project (LinkUp-TCP)

set (CMAKE_CXX_STANDARD 11)

set (Tutorial_VERSION_MAJOR 1)
set (Tutorial_VERSION_MINOR 0)

set(EXECUTABLE_OUTPUT_PATH "${PROJECT_SOURCE_DIR}/bin")
set(LIBRARY_OUTPUT_PATH "${PROJECT_SOURCE_DIR}/lib")

set(CMAKE_MODULE_PATH ${PROJECT_SOURCE_DIR}/cmake)

file(GLOB_RECURSE linkup_tcp_SOURCE_FILES "${PROJECT_SOURCE_DIR}/src"
    "*.h"
    "*.cpp"
)
add_subdirectory (${PROJECT_SOURCE_DIR}/LinkUp)

find_package(Boost REQUIRED COMPONENTS system)
include_directories(${Boost_INCLUDE_DIR})
LINK_DIRECTORIES(${Boost_LIBRARY_DIRS})

add_executable(LinkUp-TCP ${linkup_tcp_SOURCE_FILES})
target_link_libraries (LinkUp-TCP LinkUp)
target_link_libraries(LinkUp-TCP ${Boost_LIBRARIES})