import { useWorkspace } from "@/utils/WorkspaceContext"
import { Stack, Text, Button, Input, CloseButton, Textarea, Box, Card, HStack, Avatar } from "@chakra-ui/react"
import { useEffect, useState } from "react"
import { Field } from "./ui/field"
import { Field as InputField } from "@chakra-ui/react"
import { TagSelector } from "./TagSelector"
import { useTranslation } from "react-i18next"
import { NativeSelect } from "@chakra-ui/react"
import DatePicker from 'react-date-picker';
import 'react-date-picker/dist/DatePicker.css';
import { Checkbox } from "@chakra-ui/react"
import { FileManager } from "./FileManager"
import { api } from "@/api/fetchClient"
import { useParams } from "react-router"
import { MdOutlineFileDownload } from "react-icons/md"
import { FaTrashCan } from "react-icons/fa6"
import { AlertDialog } from "./AlertDialog"
import { canManageBoardContent } from "@/utils/permissions"
import { useTasks } from "@/utils/TaskContext"
import { useTags } from "@/utils/TagContext"
import { EditableTitle } from "./EditableTitle"
import { MemberSelector } from "./MemberSelector"
import { ConsoleLogger } from "@microsoft/signalr/dist/esm/Utils"

const priorities = [0, 1, 2, 3]

export function ViewTaskDialog({ task, onUpdateTask = () => { } }) {
    const { removeTag, tags } = useTags()
    const { t, i18n } = useTranslation()
    const { workspaceId, boardId } = useParams()
    const { members, role } = useWorkspace()
    const [error, setError] = useState(false)
    const [selectDueDate, setSelectDueDate] = useState(task?.dueDate ? true : false)
    const [selectedTags, setSelectedTags] = useState(tags.filter(t => task?.tagsId?.includes(t.id)))
    const [newTask, setNewTask] = useState(task)
    const [attachments, setAttachments] = useState([])
    const { tasks, allowedExtensions } = useTasks()
    const [selectedMember, setSelectedMember] = useState(null)
    const [permanentDelete, setPermanentDelete] = useState([])

    useEffect(() => {
        setSelectedMember(members.find(m => m.id == task.assigneeId) ?? null)
    }, [members, task])

    useEffect(() => {
        api.get(`/attachments?taskId=${task.id}`, workspaceId).then(res => setAttachments(res))
    }, []);

    function handleRemove(tag) {

        if (!tasks.some(e => e.id !== task.id && e.tagsId.some(t => t == tag.id))) {
            setPermanentDelete([...permanentDelete, tag.id])
        }
        setSelectedTags(selectedTags.filter(e => e.id != tag.id))
    }

    function handleDeleteTask() {
        api.delete("/tasks/" + task.id, workspaceId, boardId).then(() => console.log("task delete successfully"))
    }

    function handleUpdateTask() {
        permanentDelete
            .filter(p => !selectedTags.includes(p))
            .forEach(p => removeTag(p));

        onUpdateTask({
            ...newTask,
            priority: newTask.priority,
            title: newTask.title,
            description: newTask.description,
            assigneeId: selectedMember?.id ?? null,
            tagsId: selectedTags.map(e => e.id),
            dueDate: selectDueDate ? (newTask?.dueDate ?? new Date()) : null
        })
    }

    function arraysEqualUnordered(a, b) {
        if (a.length !== b.length) return false;
        const sortedA = [...a].sort((x, y) => x - y);
        const sortedB = [...b].sort((x, y) => x - y);
        return sortedA.every((v, i) => v === sortedB[i]);
    }

    function handleFileUpload(file) {

        const formData = new FormData()

        formData.append("file", file);
        console.log(file)
        api.postFormData(`/attachments?taskId=${task.id}`, formData, workspaceId).then(res => setAttachments([...attachments, res]))
    }

    async function handleDownload(id) {
        const response = await api.getBlob(`/attachments/${id}`, workspaceId);
        const blob = response.blob;
        const contentDisposition = response.contentDisposition;
        const downloadUrl = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = downloadUrl;

        const fileNameMatch = contentDisposition?.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
        const fileName = fileNameMatch ? fileNameMatch[1].replace(/['"]/g, '') : 'downloaded_file';

        console.log(contentDisposition)

        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(downloadUrl);
    }

    function deleteAttachment(id) {
        api.delete(`/attachments/${id}`, workspaceId).then(() => setAttachments(attachments.filter(a => a.id != id)));
    }

    return <Stack spacing={4}>

        <EditableTitle
            canEdit={true}
            onContentEdit={(value) => setNewTask({ ...newTask, title: value })}
            content={newTask?.title} />

        <Field label={t("fields.labels.desc")}>
            <Textarea
                autoresize
                maxH="5lh"
                value={newTask?.description}
                placeholder={"(" + t("fields.labels.optional") + ")"}
                onChange={e => setNewTask({ ...newTask, description: e.target.value == "" ? null : e.target.value })}
            />
        </Field>

        <Field label={t("fields.labels.priority")}>

            <NativeSelect.Root size="sm" width="240px">
                <NativeSelect.Field
                    onChange={(e) => setNewTask(
                        {
                            ...newTask,
                            priority: Number(e.currentTarget.value) ?? 0
                        })}>
                    {priorities.map(p => (
                        <option key={p} value={p}>
                            {t(`fields.priority.priority${p}`)}
                        </option>
                    ))}
                </NativeSelect.Field>
                <NativeSelect.Indicator />
            </NativeSelect.Root>
        </Field>

        <Field label={t("fields.labels.dueDate")}>
            <HStack>
                <Checkbox.Root checked={selectDueDate} >
                    <Checkbox.HiddenInput onInput={() => setSelectDueDate(prev => !prev)} />
                    <Checkbox.Control>
                        <Checkbox.Indicator />
                    </Checkbox.Control>
                    <Checkbox.Label />
                </Checkbox.Root>
                <DatePicker
                    locale={i18n.language}
                    className={"chakra-ignore"}
                    disabled={selectDueDate ? false : true}
                    onChange={value => setNewTask({ ...newTask, dueDate: value })}
                    value={new Date(newTask?.dueDate ?? Date.now())}
                    minDate={new Date(task?.dueDate ?? Date.now())}
                />
            </HStack>

        </Field>

        <Field label={t("fields.labels.assignee")}>
            {selectedMember == null ? <MemberSelector members={members} onSelect={(member) => { setSelectedMember(member) }}></MemberSelector>
                :
                <HStack key={selectedMember.email} gap="4">
                    <Avatar.Root>
                        <Avatar.Fallback name={selectedMember.name} />
                    </Avatar.Root>
                    <Stack gap="0">
                        <Text fontWeight="medium">{selectedMember.name}</Text>
                        <Text color="fg.muted" textStyle="sm">
                            {selectedMember.email}
                        </Text>
                    </Stack>
                    <CloseButton onClick={() => setSelectedMember(null)} />
                </HStack>}
        </Field>

        <Field label={t("fields.labels.tags")}>
            <TagSelector
                selectedTags={selectedTags}
                onRemove={tag => handleRemove(tag)}
                onSelect={tag => setSelectedTags([...selectedTags, tag])}
            />
        </Field>

        <Button disabled={
            (newTask.title == task.title
                && newTask.description == task.description
                && newTask.priority == task.priority
                && ((selectDueDate && (newTask.dueDate ?? new Date()) == task.dueDate ) || (newTask.dueDate == null && task.dueDate == newTask.dueDate && !selectDueDate)))
                && selectedTags.length === task.tagsId.length && arraysEqualUnordered(selectedTags.map(e => e.id), task.tagsId)
            || newTask.title == ""
        }
            onClick={() => handleUpdateTask()} bg="primary"
        >{t("fields.actions.save")}</Button>


        <Box>
            <Stack>
                <Text mt={2} mb={3} fontWeight={"medium"}>{t("fields.labels.attachments")}</Text>
                {attachments.map(a => (
                    <Box key={a.id} className="attachment-tile">
                        <HStack spacing="2" mb={3} p={2}>
                            <Text fontWeight="bold">{a.originalName}</Text>
                            {a?.createdAt && <Text fontSize="sm" color="gray.500">{new Date(a.createdAt).toLocaleString(i18n.locale, { dateStyle: "short", timeStyle: "short" })}</Text>}
                            <Text fontSize="sm">{(a.sizeBytes / 1024).toFixed(2)} KB</Text>
                            <Button size={"sm"} variant={"subtle"} className="download-file" onClick={() => handleDownload(a.id)}><MdOutlineFileDownload /></Button>

                            {canManageBoardContent(role) &&
                                <AlertDialog
                                    onConfirm={() => deleteAttachment(a.id)}
                                    base={<Button variant={"subtle"} size={"sm"} color={"danger"} className="removeAttachment"><FaTrashCan /></Button>}
                                    message={t("common.confirmDeleteAttachment")}
                                    title={t("common.confirmDelete")}
                                    confirmMessage={t("fields.actions.delete")}
                                    cancelMessage={t("fields.actions.cancel")}
                                />}
                        </HStack>
                    </Box>
                ))}
            </Stack>
            {(canManageBoardContent(role) && attachments.length <= 10) && <FileManager allowedTypes={allowedExtensions} onUpload={handleFileUpload} />}
        </Box>
        <AlertDialog
            base={<Button mt={6} w={"full"} bg={"danger"}>{t("fields.labels.deleteTask")}</Button>}
            title={t("common.confirmDelete")}
            message={t("common.confirmDeleteTask")}
            onConfirm={() => handleDeleteTask()}
            confirmMessage={t("fields.actions.delete")}
            cancelMessage={t("fields.actions.cancel")}
        />
    </Stack>

}