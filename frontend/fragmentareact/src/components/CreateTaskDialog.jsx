import { useWorkspace } from "@/utils/WorkspaceContext"
import { HStack, Stack, Text, Button, Input, CloseButton, Textarea, InputGroup, Span } from "@chakra-ui/react"
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
import { useTags } from "@/utils/TagContext"

// BUG - Data in message box stays after exit

const defaultTaskState = {
    title: "",
    description: "",
    dueDate: new Date(),
    assigneeId: null,
    weight: 0,
    priority: 0,
}

const MAX_CHARACTERS_TITLE = 50
const MAX_CHARACTERS_DESC = 150

export function CreateTaskDialog({ onAddTask }) {
    const { t, i18n } = useTranslation()
    const { members } = useWorkspace()
    const [error, setError] = useState(false)
    const [selectedMember, setSelectedMember] = useState(null)
    const [selectDueDate, setSelectDueDate] = useState(false)
    const [selectedTags, setSelectedTags] = useState([])
    const [newTask, setNewTask] = useState(defaultTaskState)
    const { tasks } = useTasks();
    const { removeTag } = useTags()
    const [chars, setChars] = useState("")
    const [charsDesc, setCharsDesc] = useState("")

    const priorities = [0, 1, 2, 3]

    function resetForm() {
        setNewTask(defaultTaskState)
        setSelectedMember(null)
        setSelectDueDate(false)
        setSelectedTags([])
        setChars("")
        setCharsDesc("")
        setError(false)
    }

    function handleTaskCreate() {
        onAddTask({
            ...newTask,
            assigneeId: selectedMember?.id ?? null,
            tagsId: selectedTags.map(e => e.id),
            dueDate: selectDueDate ? newTask.dueDate : null
        })

        resetForm();
    }

    function handleRemove(tag) {
        if (!tasks.some(e => e.tagsId.some(t => t == tag.id))) {
            removeTag(tag.id)
        }

        setSelectedTags(selectedTags.filter(e => e.id != tag.id))
    }

    return <DialogRoot
        onOpenChange={(dialog) => {
            if (!dialog.open) resetForm()
        }}>
        <DialogTrigger asChild>
            <Button className="add-task" bg="primary">{t("fields.labels.addTask")}</Button>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>{t("fields.labels.addTask")}</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>

                    <InputField.Root invalid={error}>
                        <InputField.Label>{t("fields.labels.title")}</InputField.Label>
                        <InputGroup
                            endElement={
                                <Span color="fg.muted" textStyle="xs">
                                    {chars.length} / {MAX_CHARACTERS_TITLE}
                                </Span>
                            }
                        >
                            <Input
                                className="task-title"
                                value={newTask.title}
                                maxLength={MAX_CHARACTERS_TITLE}
                                onChange={e => {
                                    setError(e.target.value == "");
                                    setNewTask({ ...newTask, title: e.target.value });
                                    setChars(e.target.value.slice(0, MAX_CHARACTERS_TITLE))
                                }}
                            />
                        </InputGroup>


                        <InputField.ErrorText>{t("fields.labels.required")}</InputField.ErrorText>
                    </InputField.Root>

                    <Field label={t("fields.labels.desc")}>
                        <InputGroup

                            endAddon={
                                <Span color="fg.muted" textStyle="xs">
                                    {charsDesc.length} / {MAX_CHARACTERS_DESC}
                                </Span>
                            }>
                            <Textarea
                                size={"sm"}
                                maxLength={MAX_CHARACTERS_DESC}
                                autoresize
                                maxH="5lh"
                                value={newTask.description}
                                placeholder="Optional"
                                onChange={e => {
                                    setNewTask({ ...newTask, description: e.target.value == "" ? null : e.target.value })
                                    setCharsDesc(e.target.value.slice(0, MAX_CHARACTERS_DESC))
                                }}
                            />
                        </InputGroup>

                    </Field>

                    <Field label={t("fields.labels.tags")}>
                        <TagSelector
                            selectedTags={selectedTags}
                            onRemove={tag => handleRemove(tag)}
                            onSelect={tag => setSelectedTags([...selectedTags, tag])} />
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

                        <Checkbox.Root className="due-date-check">
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
                            value={new Date(newTask.dueDate)}
                            minDate={new Date(Date.now())}
                        />
                    </Field>

                    <Field label={t("fields.labels.assignee")}>
                        {selectedMember == null ? <MemberSelector members={members?.filter(m => m.role != "Guest")} onSelect={(member) => { setSelectedMember(member) }}></MemberSelector>
                            :
                            <HStack key={selectedMember.email} gap="4">
                                <Avatar.Root>
                                    <Avatar.Fallback name={selectedMember.name} />
                                </Avatar.Root>
                                <Stack gap="0">
                                    <Text fontWeight="medium">{selectedMember.name}</Text>
                                    <Text className="selected-email" color="fg.muted" textStyle="sm">
                                        {selectedMember.email}
                                    </Text>
                                </Stack>
                                <CloseButton className="remove-member" onClick={() => setSelectedMember(null)} />
                            </HStack>}
                    </Field>
                </Stack>
            </DialogBody>
            <DialogFooter>

                <DialogTrigger asChild>
                    <Button color="primary" variant="outline">{t("fields.actions.cancel")}</Button>
                </DialogTrigger>
                {
                    newTask?.title != "" ?
                        (<DialogTrigger asChild>
                            <Button className="create-task" onClick={() => { setError(newTask.title == ""); handleTaskCreate(); }} bg="primary">{t("fields.actions.save")}</Button>
                        </DialogTrigger>) :
                        (<Button className="create-task" onClick={() => { setError(newTask.title == ""); handleTaskCreate(); }} bg="primary">{t("fields.actions.save")}</Button>)
                }

            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
}