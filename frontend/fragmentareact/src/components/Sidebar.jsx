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
    items: workspaces.map(e => {return { label: e.name, value: e.id }})
  });

  return (
    <Box
      h="fill"
      borderRight="1px"
      borderRightColor={"primary"}
      bg={"background"}
      pt={4}
      position="sticky"
      top="0"
    >
      <VStack spacing={4} align="stretch" px={4}>
        {/* Home link for main page */}
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

        {/* Workspace section */}
        <Box>
          <Heading size="sm" mb={2}>{t("common.workspaces")}</Heading>

          <Select.Root collection={workspacesSelect} size="sm" width="320px">
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

          {/* <NativeSelect
            onChange={(e) => {
              if (e.target.value === "") return;
              const selectedWs = workspaces.find(w => w.id === Number(e.target.value))
              if (selectedWs) {
                onWorkspaceSelect(selectedWs)
              }
            }}
            value={selectedWorkspace?.id || ""}
            bg="white"
            mb={2}
            size="sm"
          >
            <option value="">{t("common.selectWorkspace")}</option>
            {workspaces.map((workspace) => (
              <option key={workspace.id} value={workspace.id}>
                {workspace.name}
              </option>
            ))}
          </NativeSelect> */}

          <Button
            leftIcon={<FiPlus />}
            colorScheme="blue"
            size="sm"
            width="full"
          >
            {t('common.createWorkspace')}
          </Button>
        </Box>

        {/* Show boards if workspace is selected */}
        {selectedWorkspace && (
          <>
            <Divider />
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
        )}
      </VStack>
    </Box>
  )
}