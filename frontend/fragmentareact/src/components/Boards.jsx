import { api } from "@/api/fetchClient";
import { canCreateBoard, canEditBoard } from "@/utils/permissions";
import { Button, HStack, Box, Input, Stack, Text, Wrap, InputGroup, Span } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { LiaDoorOpenSolid } from "react-icons/lia";
import { Heading, Badge, CloseButton } from "@chakra-ui/react";
import { Field } from "@/components/ui/field";
import { Field as InputField } from "@chakra-ui/react"
import { DialogRoot, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter, DialogActionTrigger, DialogCloseTrigger } from "./ui/dialog";
import { useWorkspace } from "@/utils/WorkspaceContext";
import { Autocomplete } from "./Autocomplete";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { Card } from "@chakra-ui/react"
import { BiSolidArchiveIn, BiSolidArchiveOut } from "react-icons/bi";
import { EmptyState } from "@chakra-ui/react"
import { HiMiniArchiveBox } from "react-icons/hi2";
import { IoTrashBin } from "react-icons/io5";
import { ExtensionSelector } from "./ExtensionSelector";
import { FaPlus } from "react-icons/fa";
import { FiPlus } from "react-icons/fi";

const MAX_CHARACTERS_TITLE = 75

const defaultState = { name: "" };

export function Boards({ id }) {
    const navigate = useNavigate()
    const [boards, setBoards] = useState([])
    const [archivedBoards, setArchivedBoards] = useState([])
    const [form, setForm] = useState(defaultState)
    const [chosenUsers, setChosenUsers] = useState([])
    const { role } = useWorkspace()
    const { t } = useTranslation()
    const [types, setTypes] = useState([])
    const [error, setError] = useState(false);
    const [chars, setChars] = useState("")
    const [immutableTypes, setImmutableTypes] = useState([])
    const { members } = useWorkspace()

    useEffect(() => {
        api.get(`/attachment-types`, id).then(res => setImmutableTypes(res[0].children));
        setTypes(immutableTypes)
    }, [boards])

    function resetForm() {
        setForm(defaultState)
        setChars("")
        setError(false)
        setChosenUsers([])
        setTypes(immutableTypes)
    }


    useEffect(() => {
        api.get(`/boards`, id).then(res => {
            setBoards(res.filter(b => b.archivedAt == null))
            setArchivedBoards(res.filter(b => b.archivedAt != null))
        })
    }, [id])

    function createBoard() {

        const allowedTypeIds = getCheckedLeafTypeIds(types);

        api.post(`/boards`, {
            name: form.name,
            guestsId: chosenUsers.map(e => e.id),
            allowedTypeIds: allowedTypeIds
        }, id).then(res => setBoards(prev => [...prev, res]))

        resetForm()
    }

    function archiveBoard(board) {
        api.put(`/boards/${board.id}`, {
            ...board, archivedAt: new Date(Date.now())
        }, id).then(res => {
            setBoards(boards.filter(b => b.id != board.id))
            setArchivedBoards(prev => [...prev, res])
        })
    }

    function restoreBoard(board) {
        api.put(`/boards/${board.id}`, {
            ...board, archivedAt: null
        }, id).then(res => {
            setBoards(prev => [...prev, res])
            setArchivedBoards(prev => prev.filter(b => b.id != board.id))
        })
    }

    function deleteBoard(boardId) {
        api.delete(`/boards/${boardId}`, id).then(() =>
            setArchivedBoards(prev => prev.filter(b => b.id != boardId)))
    }

    const getRemainingDays = (archivedAt) => {
        const archivedDate = new Date(archivedAt);
        const deletionDate = new Date(archivedDate.getTime() + 30 * 24 * 60 * 60 * 1000);
        const now = new Date();

        const diffMs = deletionDate - now;
        const daysLeft = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
        return Math.max(0, daysLeft);
    };

    function getCheckedLeafTypeIds(nodes) {
        let selectedIds = [];

        nodes.forEach(node => {

            if (node.children?.length > 0) {
                const childIds = getCheckedLeafTypeIds(node.children);
                selectedIds = [...selectedIds, ...childIds];
            } else if (node.checked) {
                selectedIds.push(node.id);
            }

        });

        return selectedIds;
    }

    return <Stack >
        <Box p={4} gap={4} >
            <HStack justify={"space-between"} p={4}>
                <Heading>{t("common.activeBoards")}</Heading>

                {canCreateBoard(role) && <DialogRoot role="dialog"
                    onOpenChange={(dialog) => {
                        if (!dialog.open) resetForm()
                    }}
                >
                    <DialogTrigger asChild>
                        <Button bg="primary" size="sm">
                            <FiPlus />{t("fields.actions.newBoard")}
                        </Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>{t("fields.labels.createBoard")}</DialogTitle>
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
                                            maxLength={MAX_CHARACTERS_TITLE}
                                            onChange={(e) => {
                                                setChars(e.target.value.slice(0, MAX_CHARACTERS_TITLE))
                                                setForm({ ...form, name: e.target.value })
                                            }
                                            } />
                                    </InputGroup>

                                    {error && <InputField.ErrorText>{t(error)}</InputField.ErrorText>}
                                </InputField.Root>
                                <Text fontWeight="medium"> {t("fields.labels.allowedAttachmentTypes")}</Text>

                                <ExtensionSelector types={types} setTypes={setTypes}></ExtensionSelector>
                            </Stack>

                        </DialogBody>

                        <DialogFooter>
                            <DialogActionTrigger asChild>
                                <Button variant="outline">{t("fields.actions.cancel")}</Button>
                            </DialogActionTrigger>
                            {
                                form.name != "" && !boards.some(w => w.name == form.name) ?
                                    (<DialogTrigger asChild>
                                        <Button onClick={() => { setError(form?.name == "" ? "fields.labels.required" : false); createBoard() }} bg="primary">{t("fields.actions.create")}</Button>
                                    </DialogTrigger>) :
                                    (<Button onClick={() => { setError(form?.name == "" ? "fields.labels.required" : "errors.workspaceExists"); }} bg="primary">{t("fields.actions.create")}</Button>)
                            }

                        </DialogFooter>

                        <DialogCloseTrigger />
                    </DialogContent>
                </DialogRoot>}
            </HStack>

            <Stack p={4}>
                {boards.map((item) => (

                    <Card.Root _hover={{ shadow: 'sm' }}
                     key={item.id} size="sm" cursor={"pointer"} onClick={() => navigate(`/workspaces/${id}/boards/${item.id}`)}>
                        <Card.Body color="fg.muted">
                            <HStack justify={"space-between"}>
                                <Text fontWeight={"semibold"} >{item.name}</Text>

                                {canEditBoard(role) && <Button bg={"gray"}
                                    onClick={(e) => {e.stopPropagation(); archiveBoard(item)}}
                                ><BiSolidArchiveIn />{t("fields.actions.archive")}</Button>}
                            </HStack>
                        </Card.Body>
                    </Card.Root>

                ))}
            </Stack>

        </Box>
        {canEditBoard(role) &&
            <Box p={4} gap={4}>
                <Heading p={4}>{t("common.archiveBoards")}</Heading>

                {archivedBoards.length == 0 ? (
                    <EmptyState.Root size="sm">
                        <EmptyState.Content>
                            <EmptyState.Indicator>
                                <HiMiniArchiveBox />
                            </EmptyState.Indicator>
                            <Stack textAlign="center">
                                <EmptyState.Description>{t("fields.labels.archivedEmptyTitle")}</EmptyState.Description>
                            </Stack>
                        </EmptyState.Content>
                    </EmptyState.Root>
                ) :

                    <Stack p={4}>
                        {archivedBoards.map((item) => (

                            <Card.Root key={item.id} size="sm">
                                <Card.Body color="fg.muted">
                                    <HStack justify={"space-between"}>
                                        <Text onClick={() => navigate(`/workspaces/${id}/boards/${item.id}`)}>{item.name}</Text>

                                        <HStack justify={"flex-end"}>
                                            <Text> {t(getRemainingDays(item.archivedAt) === 0 ? 'common.autoDeletion_0' : 'common.autoDeletion', { count: getRemainingDays(item.archivedAt) })}</Text>
                                            <Button onClick={() => deleteBoard(item.id)} bg="danger"><IoTrashBin />{t("fields.actions.delete")}</Button>
                                            <Button onClick={() => restoreBoard(item)} bg="info"><BiSolidArchiveOut />{t("fields.actions.restore")}</Button>
                                        </HStack>
                                    </HStack>
                                </Card.Body>
                            </Card.Root>

                        ))}
                    </Stack>}
            </Box>}
    </Stack>
}