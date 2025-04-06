import { api } from "@/api/fetchClient"
import { EditableTitle } from "@/components/EditableTitle"
import { canEditBoard, canManageBoardContent } from "@/utils/permissions"
import { useWorkspace } from "@/utils/WorkspaceContext"
import {
    HStack, Stack, Box, Text, Badge, Flex, Heading, Button,
    Input
} from "@chakra-ui/react"
import { Field } from "@/components/ui/field";
import { NumberInputRoot, NumberInputField } from "@/components/ui/number-input"
import { useEffect, useState, useRef } from "react"
import { useParams } from "react-router"
import {
    DialogActionTrigger,
    DialogBody,
    DialogCloseTrigger,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogRoot,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
import { useDebounce } from "@/utils/useDebounce"
import { StatusColumn } from "@/components/StatusColumn"

export function CreateStatusDialog({ onStatusCreate }) {
    const [newStatus, setNewStatus] = useState({
        name: "",
        maxTasks: null,
        colorHex: "#3182CE"
    })

    const debouncedColorChange = useDebounce((value) => {
        setNewStatus(prev => ({ ...prev, colorHex: value }));
    }, 200);

    return (<DialogRoot>
        <DialogTrigger asChild>
            <Button bg="primary">Add Status</Button>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>Add Status</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>
                    <Field label="Name">
                        <Input
                            value={newStatus.name}
                            onChange={e => setNewStatus({ ...newStatus, name: e.target.value })}
                        />
                    </Field>

                    <Field label="Max Tasks">
                        <NumberInputRoot min={0} max={50}>
                            <NumberInputField
                                value={newStatus.maxTasks || 0}
                                onChange={e => setNewStatus({ ...newStatus, maxTasks: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </NumberInputRoot>
                    </Field>

                    <Field label="Color">
                        <Input
                            type="color"
                            value={newStatus.colorHex}
                            onChange={e => debouncedColorChange(e.target.value)}
                        />
                    </Field>
                </Stack>
            </DialogBody>
            <DialogFooter>
                <DialogActionTrigger asChild>
                    <Button color="primary" variant="outline">Cancel</Button>
                </DialogActionTrigger>
                <DialogActionTrigger asChild>
                    <Button onClick={() => onStatusCreate(newStatus)} bg="primary">Create</Button>
                </DialogActionTrigger>
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
    )
}
