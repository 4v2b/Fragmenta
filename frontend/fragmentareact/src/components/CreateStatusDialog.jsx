import {
    Stack, Button,
    Input,
    InputGroup,
    Span
} from "@chakra-ui/react"
import { NumberInput } from "@chakra-ui/react"
import { Field } from "@/components/ui/field";
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
import { Checkbox } from "@chakra-ui/react"
import { useDebounce } from "@/utils/useDebounce"
import { Field as InputField } from "@chakra-ui/react"
import { useTranslation } from "react-i18next"

const MAX_CHARACTERS_TITLE = 50

const defaultState = {
    name: "",
    maxTasks: 1,
    colorHex: "#3182CE"
}

export function CreateStatusDialog({ onStatusCreate, statusNames }) {
    const { t, i18n } = useTranslation()
    const [selectTaskLimit, setSelectTaskLimit] = useState(false)
    const [error, setError] = useState(null)
    const [newStatus, setNewStatus] = useState(defaultState)
    const [chars, setChars] = useState("")


    function resetForm() {
        setNewStatus(defaultState)
        setSelectTaskLimit(false)
        setChars("")
        setError(null)
    }

    const debouncedColorChange = useDebounce((value) => {
        setNewStatus(prev => ({ ...prev, colorHex: value }));
    }, 200);

    function handleStatusCreate() {
        if (!selectTaskLimit) 
            newStatus.maxTasks = null;
        onStatusCreate(newStatus)

        resetForm()
    }

    return (<DialogRoot
        onOpenChange={(dialog) => {
            if (!dialog.open) resetForm()
        }}
    >
        <DialogTrigger asChild>
            <Button className={"status-dialog"} bg="primary">{t("fields.actions.newStatus")}</Button>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>{t("fields.labels.addStatus")}</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>

                    <InputField.Root invalid={error != null}>
                        <InputField.Label>{t("fields.labels.name")}</InputField.Label>

                        <InputGroup
                            endElement={
                                <Span color="fg.muted" textStyle="xs">
                                    {chars.length} / {MAX_CHARACTERS_TITLE}
                                </Span>
                            }
                        >
                            <Input
                                className="status-name"
                                value={newStatus.name}
                                maxLength={MAX_CHARACTERS_TITLE}
                                onChange={e => {
                                    setNewStatus({ ...newStatus, name: e.target.value })
                                    setChars(e.target.value.slice(0, MAX_CHARACTERS_TITLE))
                                }
                                } />

                        </InputGroup>

                        <InputField.ErrorText>{t(error)}</InputField.ErrorText>
                    </InputField.Root>


                    <Field label={t("fields.labels.taskLimit")}>

                        <Checkbox.Root className="set-limit-check" size={"sm"}>
                            <Checkbox.HiddenInput onInput={() => setSelectTaskLimit(prev => !prev)} />
                            <Checkbox.Control>
                                <Checkbox.Indicator />
                            </Checkbox.Control>
                            <Checkbox.Label >{t("fields.labels.taskLimitInfo")}</Checkbox.Label>
                        </Checkbox.Root>

                        <NumberInput.Root
                            disabled={selectTaskLimit ? false : true}

                            onValueChange={e => {
                                setNewStatus({ ...newStatus, maxTasks: e.valueAsNumber }
                                );
                            }}
                            min={1} max={50}>
                            <NumberInput.Control />
                            <NumberInput.Input className="set-limit-input" value={newStatus?.maxTasks?.toString()} />
                        </NumberInput.Root>

                    </Field>

                    <Field label={t("fields.labels.color")}>
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
                    <Button color="primary" variant="outline">{t("fields.actions.cancel")}</Button>
                </DialogActionTrigger>
                {
                    newStatus?.name == "" ?
                        (<Button className="create-status" onClick={() => setError("fields.labels.required")} bg="primary">{t("fields.actions.create")}</Button>)
                        :
                        (statusNames.includes(newStatus?.name) ?
                            (<Button className="create-status" onClick={() => setError("fields.labels.statusExists")} bg="primary">{t("fields.actions.create")}</Button>) :
                            (<DialogActionTrigger asChild>
                                <Button className="create-status" onClick={() => handleStatusCreate()} bg="primary">{t("fields.actions.create")}</Button>
                            </DialogActionTrigger>)
                        )}
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
    )
}
