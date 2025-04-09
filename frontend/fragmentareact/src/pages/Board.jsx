import { api } from "@/api/fetchClient"
import { EditableTitle } from "@/components/EditableTitle"
import { canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import {
    HStack, Stack, Box, Text, Badge, Flex, Heading, Button,
    Input, Portal
} from "@chakra-ui/react"
import { Wrap, CloseButton, Table } from "@chakra-ui/react"
import { CreateStatusDialog } from "@/components/CreateStatusDialog"
import { useEffect, useState } from "react"
import { useParams } from "react-router"
import { useTags } from "@/utils/TagContext"
import { useTasks } from "@/utils/TaskContext"
import {
    DndContext, closestCenter, KeyboardSensor,
    PointerSensor, useSensor, useSensors,
    DragOverlay
} from '@dnd-kit/core';
import {
    arrayMove, SortableContext, horizontalListSortingStrategy,
} from '@dnd-kit/sortable';
import { SortableStatusColumn } from "@/components/SortableStatusColumn"
import { Drawer } from "@chakra-ui/react"
import { MemberSelector } from "@/components/MemberSelector"
import { useTransition } from "react"
import { useTranslation } from "react-i18next"
import { Guests } from "@/components/Guests"
import { BoardProvider } from "@/utils/BoardContext"

export function Board() {
    const { role } = useWorkspace()
    const { workspaceId, boardId } = useParams()
    const [board, setBoard] = useState(null)
    const { tasks, setTasks, addTask, shallowUpdateTask } = useTasks()
    const { tags } = useTags()
    const { t } = useTranslation()

    const [activeId, setActiveId] = useState(null);
    const [activeType, setActiveType] = useState(null);

    const sensors = useSensors(
        useSensor(PointerSensor),
        useSensor(KeyboardSensor)
    );

    console.log(tasks);

    useEffect(() => {
        api.get(`/boards/${boardId}`, workspaceId).then(res => setBoard(res))
    }, [])

    function handleDragStart(event) {
        const { active } = event;
        setActiveId(active.id);
        // Determine if we're dragging a column or task based on the id format
        setActiveType(active.id.includes('column-') ? 'column' : 'task');
    }

    function handleTitleChange(newTitle) {
        api.put(`/boards/${boardId}`, {
            "name": newTitle,
            "archivedAt": null
        }, workspaceId)
            .then(res => setBoard(prev => ({ ...prev, name: res.name })))
    }

    function handleAddStatus(newStatus) {
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

    function handleDragEnd(event) {
        const { active, over } = event;

        if (active.id === over.id) return;

        if (!over) return;

        // Handle column reordering
        if (activeType === 'column') {
            if (active.id !== over.id) {
                setBoard(board => {
                    const oldIndex = board.statuses.findIndex(s => `column-${s.id}` === active.id);
                    const newIndex = board.statuses.findIndex(s => `column-${s.id}` === over.id);

                    const newStatuses = arrayMove(board.statuses, oldIndex, newIndex);

                    // Update only the moved column's weight
                    // const movedColumn = newStatuses[newIndex];
                    // let newWeight = 0;

                    // if (newIndex === 0) {
                    //     // Moved to beginning
                    //     newWeight = newStatuses[1] ? newStatuses[1].weight / 2 : 500;
                    // } else if (newIndex === newStatuses.length - 1) {
                    //     // Moved to end
                    //     newWeight = newStatuses[newIndex - 1].weight + 500;
                    // } else {
                    //     // Moved between columns
                    //     newWeight = (newStatuses[newIndex - 1].weight + newStatuses[newIndex + 1].weight) / 2;
                    // }

                    // movedColumn.weight = newWeight;

                    // // Update in backend
                    // api.put(`/statuses/${movedColumn.id}`, {
                    //     ...movedColumn,
                    //     weight: newWeight
                    // }, workspaceId);

                    // return { ...board, statuses: newStatuses };

                    // Update weights
                    newStatuses.forEach((status, index) => {
                        status.weight = index * 200;

                        // Update in backend
                        api.put(`/statuses/${status.id}`, {
                            ...status,
                            weight: status.weight
                        }, workspaceId);
                    });

                    return { ...board, statuses: newStatuses };
                });
            }
        }
        // Handle task reordering
        else if (activeType === 'task') {
            const taskId = active.id.replace('task-', '');
            const targetStatusId = over.id.startsWith('task-')
                ? tasks.find(t => `task-${t.id}` === over.id).statusId
                : over.id.replace('column-', '');

            const sourceStatusId = tasks.find(t => `task-${t.id}` === active.id).statusId;

            // Only allow move if user has permission
            const task = tasks.find(t => `task-${t.id}` === active.id);
            const canMove = canManageBoardContent(role) || task.assignedUserId === currentUser.id;

            if (!canMove) return;

            if (sourceStatusId !== targetStatusId || active.id !== over.id) {

                // Update task in the state
                const updatedTasks = [...tasks];
                const movedTask = updatedTasks.find(t => `task-${t.id}` === active.id);

                // Calculate new weight
                const tasksInDestination = tasks.filter(t => t.statusId === targetStatusId);
                const overTask = tasks.find(t => `task-${t.id}` === over.id);
                const newWeight = overTask
                    ? overTask.weight + 1
                    : (tasksInDestination.length > 0
                        ? Math.max(...tasksInDestination.map(t => t.weight)) + 500
                        : 0);

                // if (tasksInDestination.length === 0) {
                //     // If column is empty, use standard increment
                //     newWeight = 500;
                // } else if (over.id.startsWith('column-')) {
                //     // Dropped directly on column - place at end
                //     newWeight = Math.max(...tasksInDestination.map(t => t.weight)) + 500;
                // } else {
                //     // Dropped on a task
                //     const overTaskId = over.id.replace('task-', '');
                //     const overTask = tasksInDestination.find(t => t.id.toString() === overTaskId);

                //     // Sort tasks by weight
                //     const sortedTasks = [...tasksInDestination].sort((a, b) => a.weight - b.weight);
                //     const overTaskIndex = sortedTasks.findIndex(t => t.id.toString() === overTaskId);

                //     if (overTaskIndex === 0) {
                //         // Dropped before first task
                //         newWeight = overTask.weight / 2;
                //     } else if (overTaskIndex === sortedTasks.length - 1) {
                //         // Dropped after last task
                //         newWeight = overTask.weight + 500;
                //     } else {
                //         // Dropped between tasks
                //         const nextTask = sortedTasks[overTaskIndex + 1];
                //         newWeight = (overTask.weight + nextTask.weight) / 2;
                //     }
                // }

                // Update task properties
                movedTask.statusId = Number(targetStatusId);
                movedTask.weight = newWeight;

                // Update the context
                // Use your task context to update tasks state

                // Update in backend
                shallowUpdateTask({
                    id: movedTask.id,
                    statusId: targetStatusId,
                    weight: newWeight
                })
            }
        }

        setActiveId(null);
        setActiveType(null);
    }

    function handleDragOver(event) {
        const { active, over } = event;

        // Only handle task dragging between columns
        if (!active || !over || !active.id.startsWith('task-')) return;

        const taskId = active.id.replace('task-', '');

        // If over a column and not another task
        if (over.id.startsWith('column-')) {
            const newStatusId = over.id.replace('column-', '');
            const task = tasks.find(t => t.id.toString() === taskId);

            // If task is already in a different column than its source
            if (task && task.statusId.toString() !== newStatusId) {
                // Update local state temporarily during drag
                setTasks(current =>
                    current.map(t =>
                        t.id.toString() === taskId
                            ? { ...t, statusId: parseInt(newStatusId) }
                            : t
                    )
                );
            }
        }
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
                    <BoardProvider>
                        <Drawer.Root size={"lg"}>
                            <Drawer.Trigger asChild>
                                <Button variant="outline" size="sm">
                                    {t("common.guests")}
                                </Button>
                            </Drawer.Trigger>
                            <Portal>
                                <Drawer.Backdrop />
                                <Drawer.Positioner>
                                    <Drawer.Content>
                                        <Drawer.Header>
                                            <Drawer.Title>{t("common.guests")}</Drawer.Title>
                                        </Drawer.Header>
                                        <Drawer.Body>
                                            <Guests />
                                        </Drawer.Body>
                                        <Drawer.CloseTrigger asChild>
                                            <CloseButton size="sm" />
                                        </Drawer.CloseTrigger>
                                    </Drawer.Content>
                                </Drawer.Positioner>
                            </Portal>
                        </Drawer.Root>
                    </BoardProvider>

                </Flex>
            )}

            <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragStart={handleDragStart}
                onDragOver={handleDragOver}
                onDragEnd={handleDragEnd}
            >
                <SortableContext
                    items={board?.statuses.map(status => `column-${status.id}`) || []}
                    strategy={horizontalListSortingStrategy}
                >
                    <HStack spacing={4} alignItems="flex-start" overflowX="auto" pb={4}>
                        {board?.statuses.map(status => (
                            <SortableStatusColumn
                                key={`column-${status.id}`}
                                id={`column-${status.id}`}
                                status={status}
                                tasks={tasks?.filter(e => e.statusId === status.id)}
                                isDisabled={!canManageBoardContent(role)}
                            />
                        ))}

                        {board?.statuses.length < 1 && (
                            <Box p={8} textAlign="center" width="100%">
                                <Text fontSize="lg" color="gray.500">{t("common.emptyBoard")}</Text>
                            </Box>
                        )}
                    </HStack>
                </SortableContext>

                <DragOverlay>
                    {activeId && activeType === 'column' && (

                        <Box
                            minWidth="280px"
                            borderWidth="1px"
                            borderRadius="lg"
                            bg="white"
                            boxShadow="xl"
                            opacity={0.8}
                        >
                            <Flex
                                justifyContent="space-between"
                                alignItems="center"
                                bg={board?.statuses.find(s => `column-${s.id}` === activeId).colorHex}
                                p={3}
                                borderTopRadius="lg"
                                color="white"
                                fontWeight="bold"
                            >
                                <Heading size="md" textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)">
                                    {board?.statuses.find(s => `column-${s.id}` === activeId)?.name}
                                </Heading>
                            </Flex>



                        </Box>
                    )}
                    {activeId && activeType === 'task' && (
                        <Box
                            p={3}
                            borderWidth="1px"
                            borderRadius="md"
                            bg="gray.100"
                            boxShadow="md"
                            opacity={0.8}
                            width="250px"
                        >
                            {tasks?.find(t => `task-${t.id}` === activeId)?.title}
                        </Box>
                    )}
                </DragOverlay>
            </DndContext>
        </Stack>
    )
}