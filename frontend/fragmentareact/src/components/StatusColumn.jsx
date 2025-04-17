import { Box, Flex, Heading, Badge, Button, Menu, Portal, HStack } from "@chakra-ui/react";
import { useWorkspace } from "@/utils/WorkspaceContext";
import { useParams } from "react-router";
import { BiDotsVerticalRounded, BiPencil, BiTrash } from "react-icons/bi";
import { EditStatusDialog } from "./EditStatusDialog";
import { CreateTaskDialog } from "./CreateTaskDialog";
import { AlertDialog } from "./AlertDialog";
import { canManageBoardContent } from "@/utils/permissions";
import { useTasks } from "@/utils/TaskContext";
import { SortableTask } from "@/components/SortableTask"

export function StatusColumn({ status, tasks }) {
    const { role } = useWorkspace();
    // const { workspaceId, boardId } = useParams();
    const { addTask,  } = useTasks()

    function handleAddTask(task) {

        const weightToAdd = tasks.length > 0
        ? tasks[tasks.length - 1].weight + 500
        : 0

        task.weight = weightToAdd;

        addTask(task, status.id)
    }

    

    return (
        <Box
            minWidth="280px"
            borderWidth="1px"
            borderRadius="lg"
            bg="white"
            boxShadow="md"
            overflow="hidden"
        >
            {/* Header */}
            <Flex
                justifyContent="space-between"
                alignItems="center"
                bg={status.colorHex}
                p={3}
                borderTopRadius="lg"
                color="white"
                fontWeight="bold"
            >
                <HStack spacing={3}>
                    <Heading size="md" textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)">
                        {status.name}
                    </Heading>

                    {status.maxTasks && (
                        <Badge bg="white" color={status.colorHex} fontWeight="bold" px={2} py={1} borderRadius="md">
                            {tasks.length} / {status.maxTasks}
                        </Badge>
                    )}
                </HStack>

                {/* Action Menu */}
                <Menu.Root closeOnSelect={false}>
                    <Menu.Trigger asChild>
                        <Button variant="ghost" size="sm" color="white" _hover={{ bg: "whiteAlpha.300" }}>
                            <BiDotsVerticalRounded />
                        </Button>
                    </Menu.Trigger>
                    <Portal>
                        <Menu.Positioner>
                            <Menu.Content boxShadow="lg" borderRadius="md" bg="white">
                                <Menu.Item>
                                    <EditStatusDialog
                                        editStatus={status}
                                        onStatusUpdate={(updateStatus) => console.log(updateStatus)}
                                        base={<HStack as="div"><BiPencil />Edit</HStack>}
                                    />
                                </Menu.Item>
                                <Menu.Item>
                                    <AlertDialog
                                        onConfirm={() => { }}
                                        base={<HStack as="div" color="red.500"><BiTrash />Delete</HStack>}
                                        message="Are you sure you want to delete this status?"
                                        title="Confirm Deletion"
                                        confirmMessage="Delete"
                                        cancelMessage="Cancel"
                                    />
                                </Menu.Item>
                            </Menu.Content>
                        </Menu.Positioner>
                    </Portal>
                </Menu.Root>
            </Flex>

            {/* Task List */}
            <Box p={3} minHeight="6em" maxHeight="36em" h="fit-content" overflowY="auto">
                {tasks?.length > 0 ? (
                    tasks?.map((task) => <SortableTask key={task.id} task={task}/>)
                ) : (
                    <Box textAlign="center" color="gray.500">No tasks yet</Box>
                )}
            </Box>

            {/* Add Task Button */}
            {canManageBoardContent(role) && (
                <Box p={3} borderTop="1px solid #ddd">
                    <CreateTaskDialog onAddTask={handleAddTask} />
                </Box>
            )}
        </Box>
    );
}
