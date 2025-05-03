import { canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import {
   Box, Badge, Flex, Heading
} from "@chakra-ui/react"
import { CreateTaskDialog } from "@/components/CreateTaskDialog";
import { useTasks } from "@/utils/TaskContext"
import {
   SortableContext,
  verticalListSortingStrategy, useSortable
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { SortableTask } from "@/components/SortableTask"
import { useTranslation } from "react-i18next"
import { RxDragHandleDots2 } from "react-icons/rx";
import { useUser } from "@/utils/UserContext";

export function SortableStatusColumn({ id, status, tasks, isDisabled }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id,
    disabled: isDisabled
  });
  const { t } = useTranslation()

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1
  };

  const { role } = useWorkspace();
  const {userId} = useUser();
  const { addTask } = useTasks()

  function handleAddTask(task) {

    const weightToAdd = tasks.length > 0
      ? tasks[tasks.length - 1].weight + 500
      : 0

    task.weight = weightToAdd;
    addTask(task, status.id)
  }

  console.log(role)

  // Check if user can drag tasks
  const canDragTask = (task) => {
    return canManageBoardContent(role) && (task.assignedUserId == null || task.assignedUserId === userId);
  };

  return (
    <Box
      flexShrink={0}
      w="300px"
      height="100%" // important so children can fill vertical space
      display="flex"
      flexDirection="column"
      borderRadius="lg"
      bg="white"
      boxShadow={isDragging ? "xl" : "md"}
      ref={setNodeRef}
      style={style}

      overflow={"auto"}
    >
      <Flex
        alignItems="center"
        // justify={"stretch"}
        bg={status.colorHex}
        p={3}
        borderTopRadius="lg"
        color="white"
        fontWeight="bold"
        justify={"space-between"}
      >
        {/* <HStack spacing={3} > */}
          <Heading className="status-heading" size="md" textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)">
            {status.name}
          </Heading>

          {status.maxTasks && (
            <Badge bg="white" color={status.colorHex} fontWeight="bold" px={2} py={1} borderRadius="md">
              {tasks.length} / {status.maxTasks}
            </Badge>
          )}
          <Box
          textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)"
          cursor={"grab"}
            {...attributes}
            {...listeners}
          ><RxDragHandleDots2/>
          </Box>
      </Flex>

      <SortableContext
        overflow="auto"
        items={tasks?.map(task => `task-${task.id}`) || []}
        strategy={verticalListSortingStrategy}
      >
        <Box
          id={id}
          p={3}
          flex="1"
          overflowY="auto"
          overflowX="hidden"
        >
          {tasks?.length > 0 ? (
            tasks
              .sort((a, b) => a.weight - b.weight)
              .map(task => (
                <SortableTask
                  key={`task-${task.id}`}
                  id={`task-${task.id}`}
                  task={task}
                  disabled={!canDragTask(task)}
                />
              ))
          ) : (
            <Box textAlign="center" color="gray.500">{t("common.noTasksYet")}</Box>
          )}
        </Box>
      </SortableContext>

      {canManageBoardContent(role) && (
        <Box p={3} borderTop="1px solid #ddd">
          <CreateTaskDialog onAddTask={handleAddTask} />
        </Box>
      )}
    </Box>
  );
}