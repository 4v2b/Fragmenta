import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { Stack, Button, Wrap, Badge, CloseButton, Table} from "@chakra-ui/react"
import { useWorkspace } from "@/utils/WorkspaceContext"
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
import { useBoard } from "@/utils/BoardContext"

export function Guests({ workspaceId }) {
    const { role, members } = useWorkspace() 
    const [chosenUsers, setChosenUsers] = useState([])
    const { t } = useTranslation()
    const {guests, addGuests, deleteGuest} = useBoard()

    function onAddGuests() {
        addGuests(chosenUsers.map(e => e.id))
        setChosenUsers([])
    }

    return (
        <Stack spacing={5} p={4}>
            <Autocomplete
                membersBlacklist={members}
                addItem={item => !chosenUsers.some(e => e.email == item.email) && setChosenUsers([...chosenUsers, item])} 
            />
            <Wrap spacing={3}>
                {chosenUsers.map(e => (
                    <Badge key={e.id} px={3} py={1} borderRadius="md" bg="gray.100" color="gray.800">
                        {e.email}
                        <CloseButton size="sm" ml={2} onClick={() => setChosenUsers(prev => prev.filter(i => i.id !== e.id))} />
                    </Badge>
                ))}
                {chosenUsers.length > 0 && (
                    <Button onClick={onAddGuests} bg="primary">{t("fields.actions.addMembers")}</Button>
                )}
            </Wrap>
            <Table.Root variant="simple">
                <Table.Header>
                    <Table.Row>
                        <Table.ColumnHeader>{t("fields.labels.username")}</Table.ColumnHeader>
                        <Table.ColumnHeader>{t("fields.labels.email")}</Table.ColumnHeader>
                        <Table.ColumnHeader textAlign="center">{t("fields.labels.kick")}</Table.ColumnHeader>
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {guests?.map((item) => (
                        <Table.Row key={item.id} _hover={{ bg: "gray.50" }}>
                            <Table.Cell>{item.name}</Table.Cell>
                            <Table.Cell>{item.email}</Table.Cell>
                            <Table.Cell textAlign="center">
                                <DialogRoot role="alertdialog">
                                    <DialogTrigger asChild>
                                        <Button 
                                            disabled={!canDeleteMember(role, "Guest")} 
                                            colorScheme="red" 
                                            size="sm"
                                        >
                                            <LiaDoorOpenSolid />
                                        </Button>
                                    </DialogTrigger>
                                    <DialogContent>
                                        <DialogHeader>
                                            <DialogTitle>{t("fields.actions.areYouSure")}</DialogTitle>
                                        </DialogHeader>
                                        <DialogBody>
                                            <p>{t("fields.actions.cannotUndone")}</p>
                                        </DialogBody>
                                        <DialogFooter>
                                            <DialogActionTrigger asChild>
                                                <Button variant="outline">{t("fields.actions.cancel")}</Button>
                                            </DialogActionTrigger>
                                            <Button onClick={() => deleteGuest(item.id)} colorScheme="red">{t("fields.actions.delete")}</Button>
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
