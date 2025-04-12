import { useEffect, useState } from "react"
import { api } from "../api/fetchClient"
import { Members } from "@/components/Members"
import { useTranslation } from "react-i18next"
import { Box, Spinner, HStack, Stack, Tabs, Input, Button, Wrap, Badge, CloseButton } from "@chakra-ui/react"
import { LuFolder, LuCheck, LuPencilLine, LuX, LuUser } from "react-icons/lu"
import { useWorkspace } from "@/utils/WorkspaceContext"
import { canEditWorkspace } from "@/utils/permissions"
import { WorkspaceGeneral } from "@/components/WorkspaceGeneral"
import { useParams, useOutletContext } from "react-router"
import { EditableTitle } from "@/components/EditableTitle"

export function Workspace() {
    const { workspaceId } = useParams()
    const { name } = useOutletContext();
    const { role } = useWorkspace()
    const { t } = useTranslation()

    async function handleTitleUpdate(name) {
        const res = await api.put(`/workspaces/${workspaceId}`, { name })
    }

    return (
        <Stack>
            <EditableTitle content={name} canEdit={canEditWorkspace(role)} onContentEdit={handleTitleUpdate} />

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
                    <Members workspaceId={workspaceId} />
                </Tabs.Content>
                <Tabs.Content value="general">
                    <WorkspaceGeneral id={workspaceId} />
                </Tabs.Content>
            </Tabs.Root>

        </Stack>)
}