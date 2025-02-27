import { api } from "@/api/fetchClient";
import { canCreateBoard } from "@/utils/permissions";
import { Button, Input, Stack, Wrap } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { LiaDoorOpenSolid } from "react-icons/lia";
import { Table, DataListRoot, DataListItem, Badge, CloseButton } from "@chakra-ui/react";
import { Field } from "./ui/field";
import { DialogRoot, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter, DialogActionTrigger, DialogCloseTrigger } from "./ui/dialog";
import { useWorkspaceRole } from "@/utils/WorkspaceContext";
import { Autocomplete } from "./Autocomplete";
import { useTranslation } from "react-i18next";

export function WorkspaceGeneral({ id }) {
    const [boards, setBoards] = useState([])
    const [form, setForm] = useState({ name: "" })
    const [chosenUsers, setChosenUsers] = useState([])
    const role = useWorkspaceRole()
    const { t } = useTranslation()

    useEffect(() => {
        api.get(`/workspaces/${id}/boards`).then(res => setBoards(res))
    }, [id])

    function createBoard() {
        api.post(`/workspaces/${id}/boards`, {
            name: form.name,
            guestsId: chosenUsers.map(e => e.id)
        }).then(res => setBoards(prev => [...prev, res]))
    }
    return <Stack>
        <Table.Root size="md">
            <Table.Header>
                <Table.Row>
                    <Table.ColumnHeader>Name</Table.ColumnHeader>
                </Table.Row>
            </Table.Header>
            <Table.Body>
                {boards.map((item) => (
                    <Table.Row key={item.id}>
                        <Table.Cell>{item.name}</Table.Cell>
                    </Table.Row>
                ))}
            </Table.Body>
        </Table.Root>

        <DialogRoot role="dialog">
            <DialogTrigger asChild>
                <Button disabled={!canCreateBoard(role)} variant="outline" size="sm">
                    <LiaDoorOpenSolid />
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Create board</DialogTitle>
                </DialogHeader>
                <DialogBody pb="8">
                    <Stack gap="4">
                        <Field label="Board Name">
                            <Input onChange={(e) => setForm({ ...form, name: e.target.value })} />
                        </Field>
                        <Field label="Guests">
                            <>
                                <Autocomplete addItem={item => !chosenUsers.some(e => e.email == item.email) && setChosenUsers([...chosenUsers, item])} />
                                <Wrap>
                                    {chosenUsers.map(e => <Badge key={e.id} >{e.email}<CloseButton onClick={() => { setChosenUsers(prev => prev.filter(i => i.id != e.id)) }} /></Badge>)}
                                </Wrap>
                            </>

                        </Field>
                    </Stack>

                </DialogBody>
                <DialogFooter>
                    <DialogActionTrigger asChild>
                        <Button variant="outline">Cancel</Button>
                    </DialogActionTrigger>
                    <DialogActionTrigger asChild>
                        <Button onClick={() => createBoard()} >Create</Button>
                    </DialogActionTrigger>

                </DialogFooter>
                <DialogCloseTrigger />
            </DialogContent>
        </DialogRoot>
    </Stack>
}