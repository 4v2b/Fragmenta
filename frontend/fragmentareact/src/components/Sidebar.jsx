import { useTranslation } from "react-i18next"
import { useState, useEffect } from "react"
import {
  Box,
  Stack,
  Button,
  Text,
  VStack,
  Heading,
  Icon,
  Flex
} from "@chakra-ui/react"
import { FiHome, FiFolder, FiPlus, FiList } from "react-icons/fi"
import { Portal, Select, createListCollection } from "@chakra-ui/react"
import { api } from "@/api/fetchClient.js"

export function Sidebar({ boards, workspaces, onWorkspaceSelect, selectedWorkspace }) {
  const { t } = useTranslation()
  const workspacesSelect = createListCollection({
    items: workspaces.map(e => { return { label: e.name, value: e.id } })
  });

  return (
    <Box
      h="full"
      borderRight="1px"
      borderRightColor={"primary"}
      bg={"background"}
      pt={4}
      position="sticky"
      top="0"
    >
      <VStack spacing={4} align="stretch" px={4}>
        <Box>

          <Select.Root
            onValueChange={(item) => onWorkspaceSelect(item.value[0])}
            value={selectedWorkspace?.name}
            collection={workspacesSelect} size="sm" width="320px">

            <Select.ClearTrigger>
              <Flex
                alignItems="center"
                p={2}
                borderRadius="md"
                cursor="pointer"
                _hover={{ bg: "gray.100" }}
                onClick={() => onWorkspaceSelect(null)}
              >
                <Icon as={FiHome} mr={2} />
                <Text fontWeight="medium">{t("common.home")}</Text>
              </Flex>
            </Select.ClearTrigger>
            <Heading size="sm" mb={2}>{t("common.workspaces")}</Heading>
            <Button gap={4}
              bg="success"
              size="sm"
              width="full"
            >
              <FiPlus />
              {t('common.createWorkspace')}
            </Button>
            <Select.Control>
              <Select.Trigger>
                <Select.ValueText placeholder={t("common.workspaceStub")} />
              </Select.Trigger>
              <Select.IndicatorGroup>
                <Select.Indicator />
              </Select.IndicatorGroup>
            </Select.Control>
            <Portal>
              <Select.Positioner>
                <Select.Content>
                  {workspacesSelect.items.map((w) => (
                    <Select.Item item={w} key={w.value}>
                      {w.label}
                      <Select.ItemIndicator />
                    </Select.Item>
                  ))}
                </Select.Content>
              </Select.Positioner>
            </Portal>
          </Select.Root>
        </Box>

        {
          selectedWorkspace && (
            <>
              <Box>
                <Heading size="sm" mb={2}>{t("common.boards")}</Heading>
                <VStack align="stretch" spacing={1}>
                  {boards?.length > 0 ? (
                    boards.map(board => (
                      <Flex
                        key={board.id}
                        alignItems="center"
                        p={2}
                        borderRadius="md"
                        cursor="pointer"
                        _hover={{ bg: "gray.100" }}
                      >
                        <Icon as={FiList} mr={2} />
                        <Text>{board.name}</Text>
                      </Flex>
                    ))
                  ) : (
                    <Text fontSize="sm" color="gray.500">
                      {t("common.noBoards")}
                    </Text>
                  )}
                  <Button
                    leftIcon={<FiPlus />}
                    variant="ghost"
                    size="sm"
                    justifyContent="flex-start"
                    mt={1}
                  >
                    {t('common.createBoard')}
                  </Button>
                </VStack>
              </Box>
            </>
          )
        }
      </VStack >
    </Box >
  )
}