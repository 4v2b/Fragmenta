import { Box, Text, HStack, Badge, VStack, Avatar, Tag, Tooltip } from "@chakra-ui/react";
import { useWorkspace } from "@/utils/WorkspaceContext";
import { useTranslation } from "react-i18next";
import { useTags } from "@/utils/TagContext";
import { HiCalendar, HiTag } from "react-icons/hi";
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
import { RxDragHandleDots2 } from "react-icons/rx";

export function SortableTask({ id, task, disabled }) {
  const { members } = useWorkspace();
  const { tags } = useTags();
  const { t, i18n } = useTranslation();
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id,
    disabled
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
    marginBottom: '8px'
  };

  const assignee = members.find(e => e.id == task?.assigneeId) ?? null;
  const dueDate = task.dueDate ? new Intl.DateTimeFormat(i18n.language, {
    month: "short", day: "2-digit", year: "numeric"
  }).format(new Date(task.dueDate)) : null;

  return (
    <Box ref={setNodeRef} style={style} 
    //{...attributes} {...listeners} Moved to a grabbing box
      p={3}
      borderWidth="1px"
      borderRadius="md"
      bg={isDragging ? "gray.100" : "white"}
      boxShadow={isDragging ? "md" : "sm"}
      w="full"
    >
      <VStack align="start" spacing={2} w="full">

        <HStack
          alignItems="center"
          justify={"space-between"}
        >
          <Box
            textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)"
            cursor={"grab"}
            {...attributes}
            {...listeners}
          ><RxDragHandleDots2 />
          </Box>

          <Text fontWeight="medium" fontSize="md">{task.title}</Text>
        </HStack>

        {task.description && (
          <Text fontSize="sm" color="gray.600" noOfLines={2}>{task.description}</Text>
        )}

        {
          dueDate && <HStack justify="space-between" w="full">
            <Badge variant="solid" colorPalette="green">
              <HiCalendar />
              {dueDate}
            </Badge>
          </HStack>
        }

        {assignee && (

          <HStack>

            <Avatar.Root size="xs">
              <Avatar.Fallback name={assignee.name} />
            </Avatar.Root>
            <Badge>{assignee.name}</Badge>
          </HStack>

        )}

        {task.tagsId.length > 0 && (
          <HStack spacing={1}>
            {task.tagsId.map(t => {
              const tagName = tags.find(e => e.id == t)?.name || t;
              return (
                <Tag.Root key={t} size="sm" color="primary" variant="outline">
                  <Tag.StartElement>
                    <HiTag />
                  </Tag.StartElement>
                  <Tag.Label>{tagName}</Tag.Label>
                </Tag.Root>
              );
            })}
          </HStack>
        )}
      </VStack>
    </Box>
  );
};
