import { api } from "@/api/fetchClient"
import { EditableTitle } from "@/components/EditableTitle"
import { canEditBoard, canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import { HStack, Stack, Tag, Box, Text, Badge, Flex, Heading, Button, Input, Checkbox, CloseButton } from "@chakra-ui/react"
import { useEffect, useState } from "react"
import { useParams } from "react-router"
import { DialogRoot, DialogTrigger, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter, DialogActionTrigger, DialogCloseTrigger } from "./ui/dialog"
import { Field } from "./ui/field"
import { NumberInputRoot, NumberInputField } from "./ui/number-input"
import DatePicker from "react-multi-date-picker";
import { MemberSelector } from "./MemberSelector"
import { Avatar } from "@chakra-ui/react"
import { useTags } from "@/utils/TagContext"
import { TagSelector } from "./TagSelector"

// BUG - Data in message box stays after exit

export function CreateTaskDialog({ onAddTask }) {
    const { members } = useWorkspace()
    const [selectedMember, setSelectedMember] = useState(null)
    const [selectedTags, setSelectedTags] = useState([])
    const [newTask, setNewTask] = useState({
        title: "",
        description: "",
        dueDate: "2025-03-09T13:18:14.762Z",
        statusId: null,
        assigneeId: 0,
        weight: 0,
        prioriry: 0,
    })

    console.log("selected tags", selectedTags)

    function handleAssigneeSelect() {
        onAddTask({ 
            ...newTask, 
            assigneeId: selectedMember.id, 
            tagsId: selectedTags.map(e => e.id) 
        })
    }

    return <DialogRoot>
        <DialogTrigger asChild>
            <Button colorScheme="blue">Add Task</Button>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>Add Task</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>
                    <Field label="Title">
                        <Input
                            value={newTask.title}
                            onChange={e => setNewTask({ ...newTask, title: e.target.value })}
                        />
                    </Field>

                    <Field label="Description">
                        <Input
                            value={newTask.description}
                            placeholder="Optional"
                            onChange={e => setNewTask({ ...newTask, description: e.target.value == "" ? null : e.target.value })}
                        />
                    </Field>

                    <Field label="Tags">
                        <TagSelector selectedTags={selectedTags} onSelect={tag => { console.log("Selected ", tag); setSelectedTags([...selectedTags, tag]) }}></TagSelector>
                    </Field>

                    <Field label="Priority">
                        <NumberInputRoot min={0} max={4}>
                            <NumberInputField
                                value={newTask.priority || 0}
                                onChange={e => setNewTask({ ...newTask, priority: e.target.value ? parseInt(e.target.value) : 0 })}
                            />
                        </NumberInputRoot>
                    </Field>

                    <Field label="Due date">
                        {/* <DatePicker
                            format="YYYY/M/D"
                            onChange={e => setNewTask({ ...newTask, dueDate: e.target.value ? new Date(e.target.value) : null })}
                        /> */}
                    </Field>

                    <Field label="Assignee">
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
                <DialogActionTrigger asChild>
                    <Button variant="outline">Cancel</Button>
                </DialogActionTrigger>
                <DialogActionTrigger asChild>
                    <Button onClick={() => handleAssigneeSelect()} colorScheme="blue">Save</Button>
                </DialogActionTrigger>
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
}