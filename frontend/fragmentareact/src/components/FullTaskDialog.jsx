import { useWorkspace } from "@/utils/WorkspaceContext"
import { HStack, Stack, Text, Button, Input, CloseButton, Textarea } from "@chakra-ui/react"
import { useState } from "react"
import { DialogRoot, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter, DialogCloseTrigger } from "./ui/dialog"
import { Field } from "./ui/field"
import { Field as InputField } from "@chakra-ui/react"
import { MemberSelector } from "./MemberSelector"
import { Avatar } from "@chakra-ui/react"
import { TagSelector } from "./TagSelector"
import { useTranslation } from "react-i18next"
import { NativeSelect } from "@chakra-ui/react"
import DatePicker from 'react-date-picker';
import 'react-date-picker/dist/DatePicker.css';
import { Checkbox } from "@chakra-ui/react"
import { useTasks } from "@/utils/TaskContext"
import { FileManager } from "./FileManager"

// BUG - Data in message box stays after exit

export function FullTaskDialog({ task, onUpdateTask }) {
    const { t, i18n } = useTranslation()
    const { members } = useWorkspace()
    const {allowedAttachmentTypes} = useTasks()
    const [error, setError] = useState(false)
    const [selectedMember, setSelectedMember] = useState(members.find(e => e.id == task.assigneeId) ?? null)
    const [selectDueDate, setSelectDueDate] = useState(false)
    const [selectedTags, setSelectedTags] = useState([])
    const [updatedTask, setUpdatedTask] = useState(task)

    Array(0)

    const priorities = [0, 1, 2, 3]

    function handleUpdateTask() {
        onUpdateTask({
            ...updatedTask,
            assigneeId: selectedMember?.id ?? null,
            tagsId: selectedTags.map(e => e.id),
            dueDate: selectDueDate ? updatedTask.dueDate : null
        })
    }

    return <DialogRoot>
        <DialogTrigger asChild>
            <Button colorScheme="blue">{t("fields.labels.addTask")}</Button>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>{t("fields.labels.addTask")}</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>

                    <InputField.Root invalid={error}>
                        <InputField.Label>{t("fields.labels.title")}</InputField.Label>
                        <Input
                            value={updatedTask.title}
                            onChange={e => { setError(e.target.value == ""); setUpdatedTask({ ...updatedTask, title: e.target.value }) }}
                        />
                        <InputField.ErrorText>{t("fields.labels.required")}</InputField.ErrorText>
                    </InputField.Root>

                    <Field label={t("fields.labels.desc")}>
                    <Textarea
                    autoresize
                    maxH="5lh"
                            value={updatedTask.description}
                            placeholder="Optional"
                            onChange={e => setUpdatedTask({ ...updatedTask, description: e.target.value == "" ? null : e.target.value })}
                        />
                    </Field>

                    <Field label={t("fields.labels.tags")}>
                        <TagSelector selectedTags={selectedTags} onSelect={tag => { console.log("Selected ", tag); setSelectedTags([...selectedTags, tag]) }}></TagSelector>
                    </Field>

                    <Field label={t("fields.labels.priority")}>

                        <NativeSelect.Root size="sm" width="240px">
                            <NativeSelect.Field
                                onChange={(e) => setUpdatedTask(
                                    {
                                        ...updatedTask,
                                        priority: Number(e.currentTarget.value) ?? 0
                                    })}
                               >

                                {priorities.map(p => (
                                    <option value={p}>
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
                            onChange={value => setUpdatedTask({ ...updatedTask, dueDate: value })}
                            value={new Date(updatedTask.dueDate)}
                            minDate={new Date(Date.now())}
                        />



                    </Field>

                    <Field label={t("fields.labels.assignee")}>
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
                    </Field>

                    <Field label={t("fields.labels.attachments")}>
                        <FileManager></FileManager>
                    </Field>
                </Stack>
            </DialogBody>
            <DialogFooter>

                <DialogTrigger asChild>
                    <Button color="primary" variant="outline">{t("fields.actions.cancel")}</Button>
                </DialogTrigger>
                {
                    updatedTask?.title != "" ?
                        (<DialogTrigger asChild>
                            <Button onClick={() => { setError(updatedTask.title == ""); handleUpdateTask(); }} color="primary">{t("fields.actions.save")}</Button>
                        </DialogTrigger>) :
                        (<Button onClick={() => { setError(updatedTask.title == ""); handleUpdateTask(); }} color="primary">{t("fields.actions.save")}</Button>)
                }

            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
}