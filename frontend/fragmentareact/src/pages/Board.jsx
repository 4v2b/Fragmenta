import { api } from "@/api/fetchClient"
import { EditableTitle } from "@/components/EditableTitle"
import { canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import {
    HStack, Stack, Box, Text, Badge, Flex, Heading, Button,
    Input
} from "@chakra-ui/react"
import { useEffect, useState, useRef } from "react"
import { useParams } from "react-router"
import { StatusColumn } from "@/components/StatusColumn"
import { CreateStatusDialog } from "@/components/CreateStatusDialog"
import { Menu, Portal, Show } from "@chakra-ui/react"
import { Tooltip } from "@/components/ui/tooltip"
import { useTags } from "@/utils/TagContext"
import { useTasks } from "@/utils/TaskContext"


export function Board() {
    const { role } = useWorkspace()
    const { workspaceId, boardId } = useParams()
    const [board, setBoard] = useState(null)
    const { tasks, addTask } = useTasks()
    const { tags } = useTags()

    console.log("tasks:", tasks)

    useEffect(() => {
        api.get(`/boards/${boardId}`, workspaceId).then(res => setBoard(res))
    }, [])

    useEffect(() => {
        api.get(`/tags?boardId=${boardId}`, workspaceId).then(res => setTags(res))
    }, [])

    function handleTitleChange(newTitle) {
        api.put(`/boards/${boardId}`, {
            "name": newTitle,
            "archivedAt": null
        }, workspaceId)
            .then(res => setBoard(prev => ({ ...prev, name: res.name })))
    }

    function handleAddStatus(newStatus) {
        // Calculate weight based on previous status
        const weightToAdd = board.statuses.length > 0
            ? board.statuses[board.statuses.length - 1].weight + 200
            : 200

        const statusToAdd = {
            ...newStatus,
            weight: weightToAdd
        }

        api.post(`/statuses?boardId=${boardId}`, statusToAdd, workspaceId)
            .then(res => {
                setBoard({ ...board, statuses: [...board.statuses, res] })
                setNewStatus({
                    name: "",
                    maxTasks: null,
                    colorHex: "#3182CE"
                })
            })
    }

    return (
        <Stack spacing={4} p={4}>
            {board && (
                <Flex justifyContent="space-between" alignItems="center">
                    <EditableTitle
                        content={board?.name}
                        onContentEdit={handleTitleChange}
                        canEdit={canManageBoardContent(role)}
                        fontSize="2xl"
                    />

                    {canManageBoardContent(role) && <CreateStatusDialog onStatusCreate={handleAddStatus} />}
                </Flex>
            )}

            <HStack spacing={4} alignItems="flex-start" overflowX="auto" pb={4}>
                {board?.statuses.map(status => (
                    <StatusColumn key={status.Id} status={status} />
                ))}

                {board?.statuses.length < 1 && (
                    <Box p={8} textAlign="center" width="100%">
                        <Text fontSize="lg" color="gray.500">No statuses found</Text>
                    </Box>
                )}
            </HStack>
        </Stack>
    )
}
