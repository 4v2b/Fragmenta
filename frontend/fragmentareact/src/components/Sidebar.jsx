import { useTranslation } from "react-i18next"
import {
  Box,
  Button,
  Text,
  VStack,
  Heading,
  Icon,
  Flex,
  InputGroup,
  Span,
} from "@chakra-ui/react"
import { FiHome, FiFolder, FiPlus, FiList } from "react-icons/fi"
import { Portal, createListCollection } from "@chakra-ui/react"
import { api } from "@/api/fetchClient.js"
import { Select, Stack, Field, Input } from "@chakra-ui/react"
import { Field as InputField } from "@chakra-ui/react"
import { DialogRoot, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter, DialogActionTrigger, DialogCloseTrigger } from "./ui/dialog";
import { useState } from "react"
import { useNavigate, useParams } from "react-router"
import { LuCog } from "react-icons/lu"
import { BiCog } from "react-icons/bi"

const MAX_CHARACTERS_TITLE = 75

export function Sidebar({ boards, workspaces, onWorkspaceSelect, selectedWorkspace }) {
  const { t } = useTranslation()
  const [newName, setNewName] = useState("");
  const [error, setError] = useState(false);
  const { workspaceId } = useParams();
  const workspacesSelect = createListCollection({
    items: workspaces.map(e => { return { label: e.name, value: e.id } })
  });
  const navigate = useNavigate();
  const [chars, setChars] = useState("")

  function createWorkspace() {
    api.post("/workspaces", { name: newName }).then(res => navigate("/workspaces/" + res.id))
    setNewName("")
  }

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
        <Heading size="sm" mb={2}>{t("common.workspaces")}</Heading>

        <DialogRoot role="dialog"
          onOpenChange={(open) => {
            if (!open.open) setNewName("")
          }}>
          <DialogTrigger asChild>

            <Button gap={4}
              bg="success"
              size="sm"
              width="full"
            >
              <FiPlus />
              {t('common.createWorkspace')}
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>{t("common.createWorkspace")}</DialogTitle>
            </DialogHeader>
            <DialogBody pb="8">
              <Stack gap="4">

                <InputField.Root invalid={error}>
                  <InputField.Label>{t("fields.labels.name")}</InputField.Label>

                  <InputGroup
                    endElement={
                      <Span color="fg.muted" textStyle="xs">
                        {chars.length} / {MAX_CHARACTERS_TITLE}
                      </Span>
                    }
                  >
                    <Input
                      value={newName}
                      onChange={e => {
                        setError(e.target.value == "" ? "fields.labels.required" : false);
                        setNewName(e.target.value)
                        setChars(e.target.value.slice(0, MAX_CHARACTERS_TITLE))
                      }}
                    />

                  </InputGroup>


                  {error && <InputField.ErrorText>{t(error)}</InputField.ErrorText>}
                </InputField.Root>
              </Stack>

            </DialogBody>
            <DialogFooter>
              <DialogActionTrigger asChild>
                <Button variant="outline">{t("fields.actions.cancel")}</Button>
              </DialogActionTrigger>
              {
                newName != "" && !workspaces.some(w => w.name == newName) ?
                  (<DialogTrigger asChild>
                    <Button onClick={() => { setError(newName == "" ? "fields.labels.required" : false); createWorkspace() }} bg="primary">{t("fields.actions.create")}</Button>
                  </DialogTrigger>) :
                  (<Button onClick={() => { setError(newName == "" ? "fields.labels.required" : "errors.workspaceExists"); }} bg="primary">{t("fields.actions.create")}</Button>)
              }

            </DialogFooter>
            <DialogCloseTrigger />
          </DialogContent>
        </DialogRoot>


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


      <Button p={6} position={"absolute"} variant={"ghost"} bottom={0} width={"100%"} className="tome" onClick={() => navigate("/me")}>{t("fields.labels.settings")} <BiCog/></Button>
    </Box >
  )
}