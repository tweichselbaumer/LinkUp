set(linkup_SOURCE_FILES1
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/CRC16.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/CRC16.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpNode.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpNode.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpRaw.cpp"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/LinkUpRaw.h"
  "${PROJECT_SOURCE_DIR}/../../LinkUp.Cpp/Platform.h"
)

file( COPY ${linkup_SOURCE_FILES1} DESTINATION "${PROJECT_SOURCE_DIR}/lib" NO_SOURCE_PERMISSIONS )

set(linkup_SOURCE_FILES
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/CRC16.cpp"
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/CRC16.h"
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/LinkUpNode.cpp"
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/LinkUpNode.h"
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/LinkUpRaw.cpp"
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/LinkUpRaw.h"
  "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/Platform.h"
)

add_library(LinkUp ${linkup_SOURCE_FILES})
target_include_directories(LinkUp PUBLIC "${PROJECT_SOURCE_DIR}/lib/LinkUp.Cpp/")