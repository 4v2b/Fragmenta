import { canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import {
  Box, Badge, Flex, Heading,
  HStack
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
import { EditStatusDialog } from "./EditStatusDialog";
import { BiPencil } from "react-icons/bi";
import { useParams } from "react-router";
import { api } from "@/api/fetchClient";

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

  const { workspaceId, boardId } = useParams()
  const { role } = useWorkspace();
  const { userId } = useUser();
  const { addTask } = useTasks()

  function handleAddTask(task) {

    const weightToAdd = tasks.length > 0
      ? tasks[tasks.length - 1].weight + 2000
      : 0

    task.weight = weightToAdd;
    addTask(task, status.id)
  }

  function handleUpdateStatus(updatedStatus) {
    api.put("/statuses/" + status.id, { ...updatedStatus, weight: status.weight }, workspaceId, boardId).then(res => console.log("success", res));
  }

  function handleDeleteStatus() {
    api.delete("/statuses/" + status.id, workspaceId, boardId).then(_ => console.log("successfully deleted"));
  }

  const canDragTask = (task) => {
    return canManageBoardContent(role) && (task.assignedUserId == null || task.assignedUserId === userId);
  };

  return (
    <Box
      className="status-column"
      flexShrink={0}
      w="300px"
      maxHeight="calc(100vh - 210px)"
      display="flex"
      flexDirection="column"
      borderRadius="lg"
      bg="white"
      boxShadow={isDragging ? "xl" : "md"}
      ref={setNodeRef}
      style={style}

    >
      <Flex
        alignItems="center"
        bg={status.colorHex}
        p={3}
        borderTopRadius="lg"
        color="white"
        fontWeight="bold"
        justify={"space-between"}
      >
        <HStack>
          <Heading className="status-heading" size="md" textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)">
            {status.name}
          </Heading>
          {canManageBoardContent(role) && <EditStatusDialog
            editStatus={status}
            onStatusDelete={handleDeleteStatus}
            onStatusUpdate={(updateStatus) => handleUpdateStatus(updateStatus)}
            base={<BiPencil cursor={"pointer"} />}
          />}
        </HStack>

        {status.maxTasks && (
          <Badge className="task-limit-badge" bg="white" color={status.colorHex} fontWeight="bold" px={2} py={1} borderRadius="md">
            {tasks.length} / {status.maxTasks}
          </Badge>
        )}
        <Box
          className="column-drag-handle"
          textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)"
          cursor={"grab"}
          {...attributes}
          {...listeners}
        ><RxDragHandleDots2 />
        </Box>
      </Flex>

      <SortableContext
        overflow="hidden"
        items={tasks?.map(task => `task-${task.id}`) || []}
        strategy={verticalListSortingStrategy}
      >
        <Box
          id={id}
          p={3}
          flex="1"
          overflowY="auto"
          style={{
            "&::-webkit-scrollbar": {
              width: "4px"
            },
            "&::-webkit-scrollbar-thumb": {
              backgroundColor: "rgba(160,160,160,0.3)",
              borderRadius: "8px"
            },
            "&::-webkit-scrollbar-track": {
              backgroundColor: "transparent"
            },
            "&:hover::-webkit-scrollbar-thumb": {
              backgroundColor: "rgba(160,160,160,0.5)"
            },
            // Додаємо тінь для переходу
            maskImage: "linear-gradient(to bottom, transparent, black 1%, black 99%, transparent)",
            WebkitMaskImage: "linear-gradient(to bottom, transparent, black 1%, black 99%, transparent)"
          }}
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