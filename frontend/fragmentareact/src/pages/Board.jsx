import { api } from "@/api/fetchClient"
import { EditableTitle } from "@/components/EditableTitle"
import { canCreateBoard, canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import {
    HStack, Stack, Box, Text, Badge, Flex, Heading, Button,
    Input, Portal,
    Dialog,
    Breadcrumb,
    Span
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
    DragOverlay, MeasuringStrategy,
    rectIntersection
} from '@dnd-kit/core';
import {
    arrayMove, SortableContext, horizontalListSortingStrategy
} from '@dnd-kit/sortable';
import { SortableStatusColumn } from "@/components/SortableStatusColumn"
import { Drawer } from "@chakra-ui/react"
import { useTranslation } from "react-i18next"
import { Guests } from "@/components/Guests"
import { BoardProvider } from "@/utils/BoardContext"
import { BiCog } from "react-icons/bi"
import { ExtensionSelector } from "@/components/ExtensionSelector"
import { ViewTaskDialog } from "@/components/ViewTaskDialog"
import { useUser } from "@/utils/UserContext"
import { LuClipboardCheck, LuClipboardList, LuFolderOpen, LuHouse } from "react-icons/lu"
import TaskFieldToggle from "@/components/TaskFieldToggle"
import { DisplayProvider } from "@/utils/DisplayContext"

export function Board() {
    const { role, name } = useWorkspace()
    const { workspaceId, boardId } = useParams()
    const [board, setBoard] = useState(null)
    const { tasks, setTasks, setTypesId, addTask, shallowUpdateTask } = useTasks()
    const { tags } = useTags()
    const { t } = useTranslation()
    const [types, setTypes] = useState([])
    const [viewedTask, setViewedTask] = useState(null);
    const [open, setOpen] = useState(false);
    const { userId } = useUser();

    const [activeId, setActiveId] = useState(null);
    const [activeType, setActiveType] = useState(null);

    const sensors = useSensors(
        useSensor(PointerSensor),
        useSensor(KeyboardSensor)
    );

    useEffect(() => {
        api.get(`/attachment-types`, workspaceId).then(res => setTypes(res[0].children));
    }, [])

    useEffect(() => {
        api.get(`/boards/${boardId}`, workspaceId).then(res => { setBoard(res); setTypesId(res.allowedTypeIds) })
    }, [])

    function handleDragStart(event) {
        const { active } = event;
        setActiveId(active.id);
        setActiveType(active.id.includes('column-') ? 'column' : 'task');

        if (active.id.includes('task-')) {
            const task = tasks.find(t => `task-${t.id}` === active.id);
            const canMove = (canManageBoardContent(role) && task.assigneeId == null) || (task.assigneeId != null && task.assigneeId == userId);

            if (!canMove) {
                setActiveId(null); return
            }
        }
    }

    function handleTitleChange(newTitle) {
        api.put(`/boards/${boardId}`, {
            name: newTitle,
            archivedAt: null,
            allowedTypeIds: board.allowedTypeIds
        }, workspaceId)
            .then(res => setBoard(prev => ({ ...prev, name: res.name })))
    }

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


    function handleAllowedTypesChange() {
        api.put(`/boards/${boardId}`, {
            name: board.name,
            archivedAt: null,
            allowedTypeIds: getCheckedLeafTypeIds(types)
        }, workspaceId)
            .then(res => {
                setBoard(prev => ({ ...prev, allowedTypeIds: res.allowedTypeIds }))
                setTypesId(res.allowedTypeIds)
            })
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
            })
    }

    function handleDragEnd(event) {
        const { active, over } = event;

        if (!active || !over || active.id === over.id) return;

        if (activeType === 'column') {
            setBoard(board => {
                const oldIndex = board.statuses.findIndex(s => `column-${s.id}` === active.id);
                const newIndex = board.statuses.findIndex(s => `column-${s.id}` === over.id);
                const newStatuses = arrayMove(board.statuses, oldIndex, newIndex);

                newStatuses.forEach((status, index) => {
                    status.weight = index * 1000;
                    api.put(`/statuses/${status.id}`, {
                        ...status,
                        weight: status.weight
                    }, workspaceId);
                });

                return { ...board, statuses: newStatuses };
            });
        }
        else if (activeType === 'task') {
            const task = tasks.find(t => `task-${t.id}` === active.id);
            const targetStatusId = over.id.startsWith('task-')
                ? tasks.find(t => `task-${t.id}` === over.id).statusId
                : Number(over.id.replace('column-', ''));

            const targetStatus = board.statuses.find(s => s.id === targetStatusId);
            const tasksInTargetStatus = tasks.filter(t =>
                t.statusId === targetStatusId && `task-${t.id}` !== active.id
            );

            if (targetStatus.maxTasks && tasksInTargetStatus.length >= targetStatus.maxTasks) {
                return;
            }

            const canMove = (canManageBoardContent(role) && task.assigneeId == null) ||
                (task.assigneeId != null && task.assigneeId == userId);
            if (!canMove) return;

            let newWeight = 0;
            const tasksInDestination = tasks.filter(t => t.statusId === targetStatusId)
                .sort((a, b) => a.weight - b.weight);

            if (over.id.startsWith('task-')) {
                const overTaskIndex = tasksInDestination.findIndex(t => `task-${t.id}` === over.id);

                if (overTaskIndex === 0) {
                    newWeight = tasksInDestination[0].weight / 2;
                } else if (overTaskIndex === tasksInDestination.length - 1) {
                    newWeight = tasksInDestination[overTaskIndex].weight + 2000;
                } else {
                    newWeight = (tasksInDestination[overTaskIndex - 1].weight +
                        tasksInDestination[overTaskIndex].weight) / 2;
                }
            } else {
                if (tasksInDestination.length === 0) {
                    newWeight = 0;
                } else {
                    newWeight = tasksInDestination[tasksInDestination.length - 1].weight + 2000;
                }
            }

            shallowUpdateTask({
                id: task.id,
                statusId: Number(targetStatusId),
                weight: newWeight
            });
        }

        setActiveId(null);
        setActiveType(null);
    }

    function handleDragOver(event) {
        const { active, over } = event;

        if (!active || !over || !active.id.startsWith('task-')) return;

        const taskId = active.id.replace('task-', '');

        if (over.id.startsWith('column-')) {
            const newStatusId = over.id.replace('column-', '');
            const task = tasks.find(t => t.id.toString() === taskId);

            const targetStatus = board.statuses.find(s => s.id.toString() === newStatusId);
            const tasksInTargetStatus = tasks.filter(t => t.statusId.toString() === newStatusId);

            if (targetStatus.maxTasks && tasksInTargetStatus.length >= targetStatus.maxTasks) {
                return;
            }

            const canMove = (canManageBoardContent(role) && task.assigneeId == null) ||
                (task.assigneeId != null && task.assigneeId == userId);
            if (!canMove) {
                return;
            }

            if (task && task.statusId.toString() !== newStatusId) {
                shallowUpdateTask({
                    id: task.id,
                    statusId: parseInt(newStatusId),
                    weight: task.weight
                });

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
        <DisplayProvider>
            <Stack spacing={6} pl={8} pt={4} overflow={"auto"}>
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
                            <Breadcrumb.Link href={`/workspaces/${workspaceId}`}>
                                <LuFolderOpen />{name}
                            </Breadcrumb.Link>
                        </Breadcrumb.Item>
                        <Breadcrumb.Separator />

                        <Breadcrumb.Item>
                            <LuClipboardCheck /> <Span fontWeight={"semibold"} p={2} >{board?.name}</Span>
                        </Breadcrumb.Item>
                    </Breadcrumb.List>
                </Breadcrumb.Root>
                {board && (
                    <Flex justifyContent="space-between" alignItems="center">
                        <EditableTitle
                            content={board?.name}
                            onContentEdit={handleTitleChange}
                            canEdit={canManageBoardContent(role)}
                            fontSize="2xl"
                        />

                        {canManageBoardContent(role) && <CreateStatusDialog statusNames={board?.statuses.map(e => e.name) || []} onStatusCreate={handleAddStatus} />}
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

                        {
                            canCreateBoard(role) &&

                            <Drawer.Root size={"xs"}>
                                <Drawer.Trigger asChild>
                                    <Button variant="outline" size="sm" className="allowedTypes">
                                        {t("fields.labels.allowedAttachmentTypes")}
                                        <BiCog />
                                    </Button>
                                </Drawer.Trigger>
                                <Portal>
                                    <Drawer.Backdrop />
                                    <Drawer.Positioner>
                                        <Drawer.Content>
                                            <Drawer.Context>
                                                {(store) => (
                                                    <>
                                                        <Drawer.Header>
                                                            <Drawer.Title>{t("fields.labels.allowedAttachmentTypes")}</Drawer.Title>
                                                        </Drawer.Header>
                                                        <Drawer.Body>
                                                            <ExtensionSelector types={types} setTypes={setTypes} presetTypes={board.allowedTypeIds} ></ExtensionSelector>
                                                        </Drawer.Body>
                                                        <Drawer.Footer>
                                                            <Button onClick={() => store.setOpen(false)} color="primary" variant="outline">{t("fields.actions.cancel")}</Button>
                                                            <Button className="submit-allowed-files" onClick={() => { handleAllowedTypesChange(); store.setOpen(false) }} bg="primary" >{t("fields.actions.save")}</Button>
                                                        </Drawer.Footer>
                                                    </>
                                                )}
                                            </Drawer.Context>


                                            <Drawer.CloseTrigger asChild>
                                                <CloseButton size="sm" />
                                            </Drawer.CloseTrigger>
                                        </Drawer.Content>
                                    </Drawer.Positioner>
                                </Portal>
                            </Drawer.Root>}
                        <TaskFieldToggle></TaskFieldToggle>

                    </Flex>
                )}

                <DndContext
                    sensors={sensors}
                    collisionDetection={rectIntersection}
                    onDragStart={handleDragStart}
                    onDragOver={handleDragOver}
                    onDragEnd={handleDragEnd}
                    modifiers={[]}
                    measuring={{
                        droppable: {
                            strategy: MeasuringStrategy.Always
                        }
                    }}
                    activationConstraint={{
                        distance: 3,
                        tolerance: 10
                    }}
                >
                    <SortableContext
                        items={board?.statuses.map(status => `column-${status.id}`) || []}
                        strategy={horizontalListSortingStrategy}
                    >
                        <HStack
                            style={{
                                "&::-webkit-scrollbar": {
                                    height: "4px",
                                    width: "4px"
                                },
                                "&::-webkit-scrollbar-thumb": {
                                    backgroundColor: "rgba(223, 223, 223, 0.3)",
                                    borderRadius: "8px"
                                },
                                "&::-webkit-scrollbar-track": {
                                    backgroundColor: "transparent"
                                },
                                "&:hover::-webkit-scrollbar-thumb": {
                                    backgroundColor: "rgba(160,160,160,0.5)"
                                },
                                maskImage: "linear-gradient(to right, transparent, black 1%, black 99%, transparent)",
                                WebkitMaskImage: "linear-gradient(to right, transparent, black 1%, black 99%, transparent)"
                            }}
                            spacing={4}
                            p={2}
                            justify="flex-start"
                            alignItems="flex-start"
                            overflowX="auto"
                            overflowY="hidden"
                            height="100%"
                        >
                            {board?.statuses.sort((a, b) => a.weight - b.weight).map(status => (
                                <SortableStatusColumn
                                    key={`column-${status.id}`}
                                    id={`column-${status.id}`}
                                    status={status}
                                    tasks={(tasks ?? []).filter(e => e.statusId === status.id)}
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
        </DisplayProvider>
    )
}