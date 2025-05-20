import {
    Stack, Button,
    Input,
    InputGroup,
    Spacer,
    Span,
    Checkbox,
    NumberInput,
    HStack
} from "@chakra-ui/react"
import { Field } from "@/components/ui/field";
import { Field as InputField } from "@chakra-ui/react"
import { NumberInputRoot, NumberInputField } from "@/components/ui/number-input"
import { useState } from "react"
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
import { useTranslation } from "react-i18next";
import { AlertDialog } from "./AlertDialog";

const MAX_CHARACTERS_TITLE = 50

export function EditStatusDialog({ base, editStatus, onStatusUpdate, onStatusDelete }) {
    const [status, setStatus] = useState({
        name: editStatus.name,
        maxTasks: editStatus.maxTasks,
        colorHex: editStatus.colorHex
    })
    const { t } = useTranslation()
    const [chars, setChars] = useState("" + editStatus.name.length)
    const [selectTaskLimit, setSelectTaskLimit] = useState(editStatus.maxTasks != null)

    const debouncedColorChange = useDebounce((value) => {
        setStatus(prev => ({ ...prev, colorHex: value }));
    }, 200);

    function resetForm() {
        setStatus({
            name: editStatus.name,
            maxTasks: selectTaskLimit ? (editStatus.maxTasks ?? 1) : null ,
            colorHex: editStatus.colorHex
        })
        setSelectTaskLimit(editStatus.maxTasks != null)
        setChars("" + editStatus.name.length)
    }

    return (<DialogRoot
        onOpenChange={(dialog) => {
            if (!dialog.open) resetForm()
        }}
    >
        <DialogTrigger asChild>
            <span>{base}</span>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>{t("fields.labels.editStatus")}</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>
                    <InputField.Root>
                        <InputField.Label>{t("fields.labels.name")}</InputField.Label>
                        <InputGroup
                            endElement={
                                <Span color="fg.muted" textStyle="xs">
                                    {chars.length} / {MAX_CHARACTERS_TITLE}
                                </Span>}
                        ><Input
                                maxLength={MAX_CHARACTERS_TITLE}
                                value={status?.name}
                                onChange={e => {
                                    setChars(e.target.value.slice(0, MAX_CHARACTERS_TITLE))
                                    setStatus({ ...status, name: e.target.value })
                                }}
                            /></InputGroup>
                    </InputField.Root>

                    <Field label={t("fields.labels.taskLimit")}>
                        <HStack>
                            <Checkbox.Root checked={selectTaskLimit} className="set-limit-check" size={"sm"}>
                                <Checkbox.HiddenInput onInput={() => setSelectTaskLimit(prev => !prev)} />
                                <Checkbox.Control>
                                    <Checkbox.Indicator />
                                </Checkbox.Control>
                                <Checkbox.Label />
                            </Checkbox.Root>
                            <NumberInput.Root
                                disabled={selectTaskLimit ? false : true}

                                onValueChange={e => {
                                    setStatus({ ...status, maxTasks: e.valueAsNumber }
                                    );
                                }}
                                 min={1} max={50}>
                                <NumberInput.Control />
                                <NumberInput.Input className="set-limit-input" value={status?.maxTasks?.toString() ?? '1'} />
                            </NumberInput.Root>
                        </HStack>

                    </Field>

                    <Field label={t("fields.labels.color")}>
                        <Input
                            type="color"
                            value={status?.colorHex}
                            onChange={e => debouncedColorChange(e.target.value)}
                        />
                    </Field>
                </Stack>
                <AlertDialog
                    base={<Button mt={6} w={"full"} bg={"danger"}>{t("fields.labels.deleteStatus")}</Button>}
                    message={t("common.confirmDeleteStatus")}
                    title={t("common.confirmDelete")}
                    onConfirm={() => onStatusDelete()}
                    confirmMessage={t("fields.actions.delete")}
                    cancelMessage={t("fields.actions.cancel")}
                />
            </DialogBody>
            <DialogFooter>
                <DialogActionTrigger asChild>
                    <Button variant="outline">{t("fields.actions.cancel")}</Button>
                </DialogActionTrigger>
                <DialogActionTrigger asChild>
                    <Button disabled={
                        (editStatus.name == status.name
                            && editStatus.colorHex == status.colorHex
                            && editStatus.maxTasks == status.maxTasks
                            && ((editStatus.maxTasks != null && selectTaskLimit) || (editStatus.maxTasks == null && !selectTaskLimit)))
                        || status.name == ""
                    }
                        onClick={() => onStatusUpdate({ ...status, maxTasks: selectTaskLimit ? status.maxTasks : null })} bg="primary">{t("fields.actions.save")}</Button>
                </DialogActionTrigger>
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
    )
}
