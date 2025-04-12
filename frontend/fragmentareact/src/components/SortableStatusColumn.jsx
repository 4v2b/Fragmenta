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
import { CreateTaskDialog } from "@/components/CreateTaskDialog";
import { Menu, Portal, Show } from "@chakra-ui/react"
import { Tooltip } from "@/components/ui/tooltip"
import { useTags } from "@/utils/TagContext"
import { useTasks } from "@/utils/TaskContext"
import {
  DndContext, closestCenter, KeyboardSensor,
  PointerSensor, useSensor, useSensors,
  DragOverlay
} from '@dnd-kit/core';
import {
  arrayMove, SortableContext, horizontalListSortingStrategy,
  verticalListSortingStrategy, useSortable
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { SortableTask } from "@/components/SortableTask"
import { useId } from 'react';
import { useTranslation } from "react-i18next"

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

  const { role, currentUser } = useWorkspace();
  const { addTask } = useTasks()

  function handleAddTask(task) {

    const weightToAdd = tasks.length > 0
      ? tasks[tasks.length - 1].weight + 500
      : 0

    task.weight = weightToAdd;
    addTask(task, status.id)
  }

  // Check if user can drag tasks
  const canDragTask = (task) => {
    return canManageBoardContent(role) || task.assignedUserId === currentUser.id;
  };

  return (
    <Box
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      minWidth="280px"
      borderWidth="1px"
      borderRadius="lg"
      bg="white"
      boxShadow={isDragging ? "xl" : "md"}
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
      </Flex>

      {/* Task List */}
      <SortableContext
        items={tasks?.map(task => `task-${task.id}`) || []}
        strategy={verticalListSortingStrategy}
      >
        <Box
          id={id} // This makes the column a droppable area for tasks
          p={3}
          minHeight="6em"
          maxHeight="36em"
          h="fit-content"
          overflowY="auto"
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

      {/* Add Task Button */}
      {canManageBoardContent(role) && (
        <Box p={3} borderTop="1px solid #ddd">
          <CreateTaskDialog onAddTask={handleAddTask} />
        </Box>
      )}
    </Box>
  );
}