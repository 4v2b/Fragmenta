import { Collapsible, Box, Checkbox, Text, Code } from "@chakra-ui/react";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { PiArrowElbowRightDownBold } from "react-icons/pi";

export function ExtensionSelector({ types, setTypes, presetTypes = [] }) {
    const { t } = useTranslation()

    useEffect(() => {
        if (presetTypes.length === 0) return;

        let updatedTypes = [...types];

        presetTypes.forEach(typeId => {
            updatedTypes = updateNodeState(updatedTypes, typeId, true);
        });

        const finalTypes = calculateNodeState(updatedTypes);
        setTypes(finalTypes);
    }, [presetTypes]);

    function updateNodeState(nodes, nodeId, checked) {
        return nodes?.map(node => {
            if (node.id === nodeId) {
                const updatedChildren = node.children.map(child => ({
                    ...child,
                    checked,
                    children: updateNodeState(child.children, child.id, checked)
                }));

                return { ...node, checked, children: updatedChildren };
            } else if (node.children.length > 0) {
                const updatedChildren = updateNodeState(node.children, nodeId, checked);

                const allChildrenChecked = updatedChildren.every(child => child.checked);
                const someChildrenChecked = updatedChildren.some(child => child.checked);

                return {
                    ...node,
                    checked: allChildrenChecked,
                    indeterminate: someChildrenChecked && !allChildrenChecked,
                    children: updatedChildren
                };
            }
            return node;
        });
    };

    function calculateNodeState(nodes) {
        return nodes?.map(node => {
            if (node.children.length > 0) {
                const updatedChildren = calculateNodeState(node.children);
                const allChildrenChecked = updatedChildren.every(child => child.checked);
                const someChildrenChecked = updatedChildren.some(child => child.checked);

                return {
                    ...node,
                    checked: allChildrenChecked,
                    indeterminate: someChildrenChecked && !allChildrenChecked,
                    children: updatedChildren
                };
            }
            return node;
        });
    };

    function handleCheckboxChange(nodeId, checked) {
        const updatedTypes = updateNodeState(types, nodeId, checked);
        const finalTypes = calculateNodeState(updatedTypes);
        setTypes(finalTypes);
    };

    function renderTypeTree(nodes) {
        return (
            <Box>
                {nodes?.map(node => (
                    <Box pl={4} key={node.id}>
                        {node.children.length > 0 ?
                            (<Collapsible.Root>
                                <Checkbox.Root
                                    size="sm"
                                    p={0.5}
                                    checked={
                                        node.indeterminate ? "indeterminate"
                                            : node.checked ? true : false}
                                    onCheckedChange={(e) => handleCheckboxChange(node.id, !!e.checked)}
                                >
                                    <Checkbox.HiddenInput />
                                    <Checkbox.Control>
                                        <Checkbox.Indicator />
                                    </Checkbox.Control>
                                    <Checkbox.Label><Text fontWeight="normal">{t("fields.labels." + node.value)}</Text></Checkbox.Label>
                                </Checkbox.Root>
                                <Collapsible.Trigger cursor={"pointer"} pl={2} pt={2}><PiArrowElbowRightDownBold /></Collapsible.Trigger>
                                <Collapsible.Content>
                                    {renderTypeTree(node.children)}
                                </Collapsible.Content>
                            </Collapsible.Root>)
                            : (<Checkbox.Root
                                p={0.5}
                                size="sm"
                                checked={
                                    node.indeterminate ? "indeterminate"
                                        : node.checked ? true : false}
                                onCheckedChange={(e) => handleCheckboxChange(node.id, !!e.checked)}
                            >
                                <Checkbox.HiddenInput />
                                <Checkbox.Control>
                                    <Checkbox.Indicator />
                                </Checkbox.Control>
                                <Checkbox.Label><Code variant="surface">{node.value}</Code></Checkbox.Label>
                            </Checkbox.Root>
                            )
                        }
                    </Box>
                ))}
            </Box>
        );
    };

    return renderTypeTree(types);
} 