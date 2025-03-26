import { useWorkspace } from "@/utils/WorkspaceContext"
import { HStack, Stack, Text, Button, Input, CloseButton } from "@chakra-ui/react"
import { useEffect, useState } from "react"
import { DialogRoot, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter, DialogActionTrigger, DialogCloseTrigger } from "./ui/dialog"
import { Field } from "./ui/field"
import { Field as InputField } from "@chakra-ui/react"
import { NumberInputRoot, NumberInputField } from "./ui/number-input"
import { MemberSelector } from "./MemberSelector"
import { Avatar } from "@chakra-ui/react"
import { TagSelector } from "./TagSelector"
import { useTranslation } from "react-i18next"
import { NativeSelect } from "@chakra-ui/react"
import DatePicker from 'react-date-picker';
import 'react-date-picker/dist/DatePicker.css';
import { Checkbox } from "@chakra-ui/react"
import { useTasks } from "@/utils/TaskContext"

// BUG - Data in message box stays after exit

export function CreateTaskDialog({ onAddTask }) {
    const { t, i18n } = useTranslation()
    const { members } = useWorkspace()
    const [error, setError] = useState(false)
    const [selectedMember, setSelectedMember] = useState(null)
    const [selectDueDate, setSelectDueDate] = useState(false)
    const [selectedTags, setSelectedTags] = useState([])
    const [newTask, setNewTask] = useState({
        title: "",
        description: "",
        dueDate: new Date(),
        assigneeId: null,
        weight: 0,
        priority: 0,
    })

    const priorities = [0, 1, 2, 3]

    function handleAssigneeSelect() {
        // console.log({
        //     ...newTask,
        //     assigneeId: selectedMember?.id,
        //     tagsId: selectedTags.map(e => e.id),
        //     dueDate: selectDueDate ? newTask.dueDate : null
        // })
        onAddTask({
            ...newTask,
            assigneeId: selectedMember?.id ?? null,
            tagsId: selectedTags.map(e => e.id),
            dueDate: selectDueDate ? newTask.dueDate : null
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
                            value={newTask.title}
                            onChange={e => { setError(e.target.value == ""); setNewTask({ ...newTask, title: e.target.value }) }}
                        />
                        <InputField.ErrorText>{t("fields.labels.required")}</InputField.ErrorText>
                    </InputField.Root>

                    <Field label={t("fields.labels.desc")}>
                        <Input
                            value={newTask.description}
                            placeholder="Optional"
                            onChange={e => setNewTask({ ...newTask, description: e.target.value == "" ? null : e.target.value })}
                        />
                    </Field>

                    <Field label="Tags">
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
                                placeholder={t("fields.labels.selPriority")}>

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
                            onChange={value => setNewTask({ ...newTask, dueDate: value })}
                            value={new Date(newTask.dueDate)}
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
                </Stack>
            </DialogBody>
            <DialogFooter>

                <DialogTrigger asChild>
                    <Button variant="outline">{t("fields.actions.cancel")}</Button>
                </DialogTrigger>
                {
                    newTask?.title != "" ?
                        (<DialogTrigger asChild>
                            <Button onClick={() => { setError(newTask.title == ""); handleAssigneeSelect(); }} colorScheme="blue">{t("fields.actions.save")}</Button>
                        </DialogTrigger>) :
                        (<Button onClick={() => { setError(newTask.title == ""); handleAssigneeSelect(); }} colorScheme="blue">{t("fields.actions.save")}</Button>)
                }

            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
}