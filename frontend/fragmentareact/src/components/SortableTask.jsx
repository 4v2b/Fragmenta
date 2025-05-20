import { Box, Text, HStack, Badge, VStack, Avatar, Tag, Dialog, Button, Portal } from "@chakra-ui/react";
import { useWorkspace } from "@/utils/WorkspaceContext";
import { useTranslation } from "react-i18next";
import { useTags } from "@/utils/TagContext";
import { HiCalendar, HiTag } from "react-icons/hi";
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { RxDragHandleDots2 } from "react-icons/rx";
import { ViewTaskDialog } from "./ViewTaskDialog";
import { useDisplay } from "@/utils/DisplayContext";
import { Tooltip } from "@/components/ui/tooltip"
import { api } from "@/api/fetchClient";
import { useParams } from "react-router";

const priorityColorMap = [
  "grey.400", "red.400", "yellow.400", "green.400",
];

export function SortableTask({ id, task, disabled }) {
  const { workspaceId, boardId } = useParams();
  const { members } = useWorkspace();
  const { visibleFields } = useDisplay()
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

  console.log(task.dueDate)

  const assignee = members.find(e => e.id == task?.assigneeId) ?? null;
  const dueDate = task.dueDate ? new Intl.DateTimeFormat(i18n.language, {
    month: "short", day: "2-digit", year: "numeric"
  }).format(new Date(task.dueDate)) : null;

  function handleUpdateTask(task) {
    api.put("/tasks/" + task.id, task, workspaceId, boardId).then(() => console.log("task updated"))
  }

  return (
    <Box ref={setNodeRef} style={style}
      p={3}
      borderWidth="1px"
      borderRadius="md"
      bg={isDragging ? "gray.100" : "white"}
      boxShadow={isDragging ? "md" : "sm"}
      w="full"
    >
      <VStack align="start" spacing={2} w="full">

        <HStack
          className="task-head"
          alignItems="center"
          justify="space-between"
          textOverflow={"ellipsis"}
        >
          <Box
            className="task-drag-handle"
            textShadow="0px 1px 2px rgba(0, 0, 0, 0.4)"
            cursor={"grab"}
            {...attributes}
            {...listeners}
          ><RxDragHandleDots2 />
          </Box>

          <Dialog.Root>
            <Portal>
              <Dialog.Backdrop />
              <Dialog.Positioner>
                <Dialog.Content>
                  <Dialog.Body pt={6}>
                    <ViewTaskDialog onUpdateTask={(task) => handleUpdateTask(task)} task={task} />
                  </Dialog.Body>
                  <Dialog.Footer>
                    <Dialog.Trigger asChild>
                      <Button color="primary" variant="outline">{t("fields.actions.cancel")}</Button>
                    </Dialog.Trigger>
                  </Dialog.Footer>
                  <Dialog.CloseTrigger />
                </Dialog.Content>
              </Dialog.Positioner>
            </Portal>
            <Dialog.Trigger asChild>
              <Button
                className="task-open"
                variant="ghost"
                textAlign={"left"}
                size="xs"
                fontSize="sm"
                textWrap={"nowrap"}
                textOverflow={"ellipsis"}
                whiteSpace="normal"
                overflow="hidden"
                width="full"
                style={{
                  display: "-webkit-box",
                  WebkitLineClamp: "2",
                  WebkitBoxOrient: "vertical",
                  overflow: "hidden",
                  textOverflow: "ellipsis",
                  whiteSpace: "normal"
                }}
              >
                {task.title}</Button>
            </Dialog.Trigger>
          </Dialog.Root>
        </HStack>

        {
          (dueDate && visibleFields?.includes("dueDate")) && <HStack className="due-date" justify="space-between" w="full">
            <Badge variant="solid" colorPalette="green">
              <HiCalendar />
              {dueDate}
            </Badge>
          </HStack>
        }

        {(assignee && visibleFields?.includes("assignee")) && (

          <HStack className="assignee">

            <Avatar.Root size="xs">
              <Avatar.Fallback name={assignee.name} />
            </Avatar.Root>
            <Badge>{assignee.name}</Badge>
          </HStack>

        )}

        {(task?.priority > 0 && visibleFields?.includes("priority")) && (
          <Badge px={2} py={1} borderRadius="md" variant="subtle" colorScheme="gray">
            <HStack spacing={1}>
              <Text fontSize="xm">{t("fields.labels.priority")}</Text>
              <Tooltip
                content={t("fields.priority.priority" + task?.priority)}
              >
                <Box boxSize={3.5} borderRadius="full" bg={priorityColorMap[task.priority]} />
              </Tooltip>

            </HStack>
          </Badge>
        )}

        {(task.tagsId.length > 0 && visibleFields?.includes("tags")) && (
          <HStack spacing={1}>
            {task.tagsId.map(t => {
              const tagName = tags.find(e => e.id == t)?.name || t;
              return (
                <Tag.Root key={t} size="sm" color="green" variant="outline">
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
