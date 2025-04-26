import { useWorkspace } from "@/utils/WorkspaceContext"
import { Stack, Text, Button, Input, CloseButton, Textarea } from "@chakra-ui/react"
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

// BUG - Data in message box stays after exit

export function ViewTaskDialog({ task, onUpdateTask = () => { } }) {
    const { t, i18n } = useTranslation()
    const { workspaceId, boardId } = useParams()
    const { members } = useWorkspace()
    const [error, setError] = useState(false)
    const [selectDueDate, setSelectDueDate] = useState(task?.dueDate ? true : false)
    const [selectedTags, setSelectedTags] = useState([])
    const [newTask, setNewTask] = useState(task)
    const [attachments, setAttachments] = useState([])

    useEffect(() => {

        api.get(`/attachments?taskId=${task.id}`, workspaceId).then(res => setAttachments(res))
    }, []);

    const priorities = [0, 1, 2, 3]

    function handleAssigneeSelect() {
        onAddTask({
            ...newTask,
            assigneeId: selectedMember?.id ?? null,
            tagsId: selectedTags.map(e => e.id),
            dueDate: selectDueDate ? newTask?.dueDate : null
        })
    }

    function handleFileUpload(file) {

        const formData = new FormData()

        formData.append("file", file);
        console.log(file)
        api.postFormData(`/attachments?taskId=${task.id}`, formData, workspaceId).then(res => console.log(res))

        // try {
        //     const response = await fetch("/upload", {
        //         method: "POST",
        //         body: formData,
        //     });

        //     if (!response.ok) throw new Error("Upload failed");

        //     toaster.create({ title: t("fields.labels.uploadSuccess"), type: "success" });
        // } catch (error) {
        //     toaster.create({ title: t("fields.labels.uploadError"), type: "error" });
        // }
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
            <TagSelector selectedTags={selectedTags} onSelect={tag => { console.log("Selected ", tag); setSelectedTags([...selectedTags, tag]) }}></TagSelector>
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

        <FileManager allowedTypes={[".txt"]} onUpload={handleFileUpload} />



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