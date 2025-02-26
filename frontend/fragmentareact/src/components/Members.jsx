import { useEffect, useState } from "react"
import { api } from "@/api/fetchClient"
import { useTranslation } from "react-i18next"
import { Stack, Button, Wrap, Badge, CloseButton, Table } from "@chakra-ui/react"
import { useWorkspaceRole } from "@/utils/WorkspaceContext"
import { Autocomplete } from "@/components/Autocomplete"
import { canDeleteMember } from "@/utils/permissions"
import { LiaDoorOpenSolid } from "react-icons/lia";
import {
    DialogActionTrigger,
    DialogBody,
    DialogCloseTrigger,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogRoot,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"

export function Members({ workspaceId }) {
    const [chosenUsers, setChosenUsers] = useState([])
    const [members, setMembers] = useState([])
    const role = useWorkspaceRole()
    const { t } = useTranslation()

    useEffect(() => {
        api.get(`/workspaces/${workspaceId}/members`).then(res => setMembers(res))
    }, [])

    // function addMembers() {
    //     api.post(`/workspaces/${workspaceId}/members`, { usersId: chosenUsers.map(e => e.id) }).then(addedMembers => setMembers([addedMembers, ...members]))
    // }

    function addMembers() {
        api.post(`/workspaces/${workspaceId}/members`, { usersId: chosenUsers.map(e => e.id) })
            .then(() => api.get(`/workspaces/${workspaceId}/members`))
            .then(setMembers);
    }

    function deleteMember(id) {
        api.delete(`/workspaces/${workspaceId}/members/${id}`).then(setMembers(members.filter(e => e.id != id))).catch(e => console.log(e))
    }

    return (<Stack>
        <Autocomplete addItem={item => !chosenUsers.some(e => e.email == item.email) && setChosenUsers([...chosenUsers, item])} />
        <Wrap>
            {chosenUsers.map(e => <Badge key={e.id} >{e.email}<CloseButton /></Badge>)}
            {chosenUsers.length > 0 && <Button onClick={() => addMembers()}>{t("fields.addMembers")}</Button>}
        </Wrap>
        <Table.Root size="md">
            <Table.Header>
                <Table.Row>
                    <Table.ColumnHeader>Name</Table.ColumnHeader>
                    <Table.ColumnHeader>Email</Table.ColumnHeader>
                    <Table.ColumnHeader textAlign="end">Role</Table.ColumnHeader>
                    <Table.ColumnHeader>Kick</Table.ColumnHeader>
                </Table.Row>
            </Table.Header>
            <Table.Body>
                {members.map((item) => (
                    <Table.Row key={item.id}>
                        <Table.Cell>{item.name} { } </Table.Cell>
                        <Table.Cell>{item.email}</Table.Cell>
                        <Table.Cell>{item.role ? t(`roles.${item.role.toLowerCase()}`) : "Unknown"}</Table.Cell>
                        <Table.Cell>
                            <DialogRoot role="alertdialog">
                                <DialogTrigger asChild>
                                    <Button disabled={!canDeleteMember(role, item.role)} variant="outline" size="sm">
                                        <LiaDoorOpenSolid />
                                    </Button>
                                </DialogTrigger>
                                <DialogContent>
                                    <DialogHeader>
                                        <DialogTitle>Are you sure?</DialogTitle>
                                    </DialogHeader>
                                    <DialogBody>
                                        <p>This action cannot be undone.</p>
                                    </DialogBody>
                                    <DialogFooter>
                                        <DialogActionTrigger asChild>
                                            <Button variant="outline">Cancel</Button>
                                        </DialogActionTrigger>
                                        <Button onClick={() => deleteMember(item.id)} colorPalette="red">Delete</Button>
                                    </DialogFooter>
                                    <DialogCloseTrigger />
                                </DialogContent>
                            </DialogRoot>
                        </Table.Cell>
                    </Table.Row>
                ))}
            </Table.Body>
        </Table.Root>
    </Stack>
    )
}