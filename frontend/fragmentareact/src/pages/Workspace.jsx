import { useEffect, useState } from "react"
import { api } from "../api/fetchClient"
import { Members } from "@/components/Members"
import { useTranslation } from "react-i18next"
import { Box, Spinner, HStack, Stack, Tabs, Input, Button, Wrap, Badge, CloseButton } from "@chakra-ui/react"
import { LuFolder, LuCheck, LuPencilLine, LuX, LuUser } from "react-icons/lu"
import { useWorkspace } from "@/utils/WorkspaceContext"
import { canEditWorkspace } from "@/utils/permissions"
import { Boards } from "@/components/Boards"
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
        <Stack p={4} gap={4}>
            <EditableTitle content={name} canEdit={canEditWorkspace(role)} onContentEdit={handleTitleUpdate} />

            <Tabs.Root variant={"enclosed"} defaultValue="boards">
                <Tabs.List>
                    <Tabs.Trigger value="boards">
                        <LuFolder />
                        {t("common.boards")}
                    </Tabs.Trigger>
                    <Tabs.Trigger value="members">
                        <LuUser />
                        {t("common.members")}
                    </Tabs.Trigger>
                </Tabs.List>
                <Tabs.Content bg={"background"} value="members">
                    <Members workspaceId={workspaceId} />
                </Tabs.Content>
                <Tabs.Content bg={"background"} value="boards">
                    <Boards id={workspaceId} />
                </Tabs.Content>
            </Tabs.Root>

        </Stack>)
}