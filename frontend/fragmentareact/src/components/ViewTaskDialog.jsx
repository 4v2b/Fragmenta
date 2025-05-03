import { useWorkspace } from "@/utils/WorkspaceContext"
import { Stack, Text, Button, Input, CloseButton, Textarea, Box, Card, HStack } from "@chakra-ui/react"
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

// BUG - Data in message box stays after exit

export function ViewTaskDialog({ task, onUpdateTask = () => { } }) {
    const { t, i18n } = useTranslation()
    const { workspaceId, boardId } = useParams()
    const { members, role } = useWorkspace()
    const [error, setError] = useState(false)
    const [selectDueDate, setSelectDueDate] = useState(task?.dueDate ? true : false)
    const [selectedTags, setSelectedTags] = useState([])
    const [newTask, setNewTask] = useState(task)
    const [attachments, setAttachments] = useState([])
    const { tasks } = useTasks()
    const { removeTag } = useTags()

    useEffect(() => {

        api.get(`/attachments?taskId=${task.id}`, workspaceId).then(res => setAttachments(res))
    }, []);

    const priorities = [0, 1, 2, 3]

    function handleRemove(tag) {

        if (!tasks.some(e => e.id !== task.id && e.tagsId.some(t => t == tag.id))) {
            removeTag(tag.id)
        } else {
            setSelectedTags(selectedTags.filter(e => e != tag.id))
        }
    }


    function handleAssigneeSelect() {
        onAddTask({
            ...newTask,
            assigneeId: selectedMember?.id ?? null,
            tagsId: selectedTags.map(e => e.id),
            dueDate: selectDueDate ? newTask?.dueDate : null
        })
    }

    console.log(attachments)

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

        <InputField.Root invalid={error}>
            <InputField.Label>{t("fields.labels.title")}</InputField.Label>
            <Input
                value={newTask?.title}
                onChange={e => { setError(e.target.value == ""); setNewTask({ ...newTask, title: e.target.value }) }}
            />
            <InputField.ErrorText>{t("fields.labels.required")}</InputField.ErrorText>
        </InputField.Root>

        <Field label={t("fields.labels.desc")}>
            <Textarea
                autoresize
                maxH="5lh"
                value={newTask?.description}
                placeholder="Optional"
                onChange={e => setNewTask({ ...newTask, description: e.target.value == "" ? null : e.target.value })}
            />
        </Field>

        <Field label={t("fields.labels.tags")}>
            <TagSelector
                selectedTags={selectedTags}
                onRemove={tag => handleRemove(tag)}
                onSelect={tag => setSelectedTags([...selectedTags, tag])}
            />
        </Field>

        <Field label={t("fields.labels.priority")}>

            <NativeSelect.Root size="sm" width="240px">
                <NativeSelect.Field
                    onChange={(e) => setNewTask(
                        {
                            ...newTask,
                            priority: Number(e.currentTarget.value) ?? 0
                        })}
                >

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

            <Checkbox.Root>
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
                value={new Date(newTask?.dueDate)}
                minDate={new Date(Date.now())}
            />
        </Field>

        <Box>
            <Stack>
                {attachments.map(a => (
                    <Box key={a.id} className="attachment-tile">
                        <HStack spacing="2">
                            <Text fontWeight="bold">{a.originalName}</Text>
                            {/* <Text fontSize="sm" color="gray.500">{new Date(a.createdAt).toLocaleString()}</Text> */}
                            <Text fontSize="sm">{(a.sizeBytes / 1024).toFixed(2)} KB</Text>
                            <Button className="download-file" onClick={() => handleDownload(a.id)}><MdOutlineFileDownload /></Button>

                            {canManageBoardContent(role) &&
                                <AlertDialog
                                    onConfirm={() => deleteAttachment(a.id)}
                                    base={<Button className="removeAttachment"><FaTrashCan /></Button>}
                                    message="Are you sure you want to delete this status?"
                                    title="Confirm Deletion"
                                    confirmMessage="Delete"
                                    cancelMessage="Cancel"
                                />}


                        </HStack>
                    </Box>
                ))}
            </Stack>
            {canManageBoardContent(role) && <FileManager allowedTypes={[".txt", ".zip"]} onUpload={handleFileUpload} />}
        </Box>




        {/* <Field label={t("fields.labels.assignee")}>
                        {selectedMember == null ? <MemberSelector members={members} onSelect={(member) => { console.log("Selected ", member); setSelectedMember(member) }}></MemberSelector>
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
                    </Field> */}
    </Stack>

}