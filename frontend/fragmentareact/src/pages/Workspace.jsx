import { useEffect, useState } from "react"
import { api } from "../api/fetchClient"
import { Members } from "@/components/Members"
import { useTranslation } from "react-i18next"
import { Box, Spinner, HStack, Stack, Tabs, Input, Button, Wrap, Badge, CloseButton, Breadcrumb, Span } from "@chakra-ui/react"
import { LuFolder, LuCheck, LuPencilLine, LuX, LuUser, LuHouse, LuFolderOpen } from "react-icons/lu"
import { useWorkspace } from "@/utils/WorkspaceContext"
import { canEditWorkspace, canLeaveWorkspace } from "@/utils/permissions"
import { Boards } from "@/components/Boards"
import { useParams, useOutletContext, useNavigate } from "react-router"
import { EditableTitle } from "@/components/EditableTitle"
import { useUser } from "@/utils/UserContext"
import { AlertDialog } from "@/components/AlertDialog"

export function Workspace() {
    const { workspaceId } = useParams()
    const { name } = useOutletContext();
    const { role } = useWorkspace()
    const { t } = useTranslation()
    const { userId } = useUser();
    const navigate = useNavigate()

    const [canDelete, setCanDelete] = useState(false)

    useEffect(() => {
        api.get(`/boards`, workspaceId).then(res => {
            setCanDelete(res.every(b => b.archivedAt != null))
        })
    }, [workspaceId])

    async function handleTitleUpdate(name) {
        const res = await api.put(`/workspaces/${workspaceId}`, { name })
    }

    function handleLeave(){
        api.delete("/members/" + userId, workspaceId).then(() => navigate("/"))
    }

    function handleDelete(){
        api.delete("/workspaces/" + workspaceId).then(() => navigate("/"))
    }

    return (
        <Stack p={4} gap={4} spacing={6}>
                    <Breadcrumb.Root>
                        <Breadcrumb.List>
                            <Breadcrumb.Item>
                                <Breadcrumb.Link href="/">
                                    <LuHouse />
                                    {t("common.home")}
                                </Breadcrumb.Link>
                            </Breadcrumb.Item>
                            <Breadcrumb.Separator />
            
                            <Breadcrumb.Item>
                                <LuFolderOpen /> <Span fontWeight={"semibold"} p={2} >{name}</Span>
                            </Breadcrumb.Item>
                        </Breadcrumb.List>
                    </Breadcrumb.Root>
            <HStack >
                <EditableTitle content={name} canEdit={canEditWorkspace(role)} onContentEdit={handleTitleUpdate} />

                {canLeaveWorkspace(role) &&
                    <AlertDialog
                        onConfirm={() => handleLeave()}
                        base={<Button className="leave-workspace">{t("fields.actions.leaveWorkspace")}</Button>}
                        message="Are you sure you want to leave this workspace?"
                        title="Confirm Deletion"
                        confirmMessage="Delete"
                        cancelMessage="Cancel"
                    />

                }

                {canEditWorkspace(role) &&
                    <AlertDialog
                        onConfirm={() => handleDelete()}
                        base={<Button disabled={!canDelete}  className="delete-workspace">{t("fields.actions.deleteWorkspace")}</Button>}
                        message="Are you sure you want to delete workspace?"
                        title="Confirm Deletion"
                        confirmMessage={t("fields.actions.delete")}
                        cancelMessage={t("fields.actions.cancel")}
                    />

                }

            </HStack>


            <Tabs.Root variant={"outline"} defaultValue="boards">
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
                <Tabs.Content overflow={"auto"} bg={"background"} value="members" borderRadius={4}>
                    <Members workspaceId={workspaceId} />
                </Tabs.Content>
                <Tabs.Content overflow={"auto"} bg={"background"} value="boards"  borderRadius={4}>
                    <Boards id={workspaceId} />
                </Tabs.Content>
            </Tabs.Root>

        </Stack>)
}