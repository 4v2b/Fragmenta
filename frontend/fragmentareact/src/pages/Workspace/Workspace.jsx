import { useEffect, useState } from "react"
import "./Workspace.css"
import { api } from "../../api/fetchClient"
import { Members } from "@/components/Members"
import { ListItem } from "../../components/ListItem/ListItem"
import { useTranslation } from "react-i18next"
import { Box, Spinner, HStack, Stack, Tabs, Input, Button, Wrap, Badge, CloseButton } from "@chakra-ui/react"
import { LuFolder, LuCheck, LuPencilLine, LuX, LuUser } from "react-icons/lu"
import { Editable, IconButton, } from "@chakra-ui/react"
import { useWorkspaceRole } from "../../utils/WorkspaceContext"
import { canEditWorkspace } from "../../utils/permissions"
import { WorkspaceGeneral } from "@/components/WorkspaceGeneral"

export function Workspace({ name, id }) {
    const role = useWorkspaceRole()
    const { t } = useTranslation()

    async function handleTitleUpdate(name) {
        const res = await api.put(`/workspaces/${id}`, { name })
        console.log("Put result", res)
    }

    return (

        <Stack>
            {canEditWorkspace(role) ?
                name :
                (<Editable.Root onValueCommit={e => handleTitleUpdate(e.value)} defaultValue={name}>
                    <Editable.Preview />
                    <Editable.Input />
                    <Editable.Control>
                        <Editable.EditTrigger asChild>
                            <IconButton variant="ghost" size="xs">
                                <LuPencilLine />
                            </IconButton>
                        </Editable.EditTrigger>
                        <Editable.CancelTrigger asChild>
                            <IconButton variant="outline" size="xs">
                                <LuX />
                            </IconButton>
                        </Editable.CancelTrigger>
                        <Editable.SubmitTrigger asChild>
                            <IconButton variant="outline" size="xs">
                                <LuCheck />
                            </IconButton>
                        </Editable.SubmitTrigger>
                    </Editable.Control>
                </Editable.Root>)}

            <Tabs.Root defaultValue="members">
                <Tabs.List>
                    <Tabs.Trigger value="general">
                        <LuFolder />
                        {t("general")}
                    </Tabs.Trigger>
                    <Tabs.Trigger value="members">
                        <LuUser />
                        {t("members")}
                    </Tabs.Trigger>
                </Tabs.List>
                <Tabs.Content value="members">
                    <Members workspaceId={id}/>
                </Tabs.Content>
                <Tabs.Content value="general"><WorkspaceGeneral id={id}/></Tabs.Content>
            </Tabs.Root>

        </Stack>)
}