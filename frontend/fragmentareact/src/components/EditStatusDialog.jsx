import {
    Stack, Button,
    Input
} from "@chakra-ui/react"
import { Field } from "@/components/ui/field";
import { NumberInputRoot, NumberInputField } from "@/components/ui/number-input"
import {  useState} from "react"
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

export function EditStatusDialog({ base, editStatus, onStatusUpdate }) {
    const [status, setStatus] = useState({
        name: editStatus.name,
        maxTasks: editStatus.maxTasks,
        colorHex: editStatus.colorHex
    })


    const debouncedColorChange = useDebounce((value) => {
        setStatus(prev => ({ ...prev, colorHex: value }));
    }, 200);

    return (<DialogRoot>
        <DialogTrigger asChild>
            {base}
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>Edit Status</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>
                    <Field label="Name">
                        <Input
                            value={status?.name}
                            onChange={e => setNewStatus({ ...status, name: e.target.value })}
                        />
                    </Field>

                    <Field label="Max Tasks">
                        <NumberInputRoot min={0} max={50}>
                            <NumberInputField
                                value={status?.maxTasks || 0}
                                onChange={e => setNewStatus({ ...status, maxTasks: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </NumberInputRoot>
                    </Field>

                    <Field label="Color">
                        <Input
                            type="color"
                            value={status?.colorHex}
                            onChange={e => debouncedColorChange(e.target.value)}
                        />
                    </Field>
                </Stack>
            </DialogBody>
            <DialogFooter>
                <DialogActionTrigger asChild>
                    <Button variant="outline">Cancel</Button>
                </DialogActionTrigger>
                <DialogActionTrigger asChild>
                    <Button onClick={() => onStatusUpdate(status)} colorScheme="blue">Save</Button>
                </DialogActionTrigger>
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
    )
}
