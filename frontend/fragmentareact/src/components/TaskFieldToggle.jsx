import React, { useEffect, useState } from "react";
import { Box, Checkbox, Stack, Text } from "@chakra-ui/react";

import { Button, Menu, Portal, useCheckboxGroup } from "@chakra-ui/react"
import { HiCog } from "react-icons/hi"
import { useTranslation } from "react-i18next";
import { useDisplay } from "@/utils/DisplayContext";

const items = [
  { value: "dueDate", title: "fields.labels.dueDate" },
  { value: "priority", title: "fields.labels.priority" },
  { value: "tags", title: "fields.labels.tags" },
  { value: "assignee", title: "fields.labels.assignee" },
];

const LOCAL_STORAGE_KEY = "taskFieldVisibility";

export default function TaskFieldToggle({ onDisplayChange }) {
  const { setVisibleFields } = useDisplay();
  const group = useCheckboxGroup({ defaultValue: setState() })

  const { t } = useTranslation()
  const toggleField = (fieldId) => {
    const isChecked = group.isChecked(fieldId);
    const newValue = isChecked
      ? group.value.filter(v => v !== fieldId)
      : [...group.value, fieldId];

    group.setValue(newValue);
    setVisibleFields(newValue)
  };

  function setState() {
    const saved = localStorage.getItem(LOCAL_STORAGE_KEY);
    return saved ? JSON.parse(saved) : items.map(f => f.value);
  };

  return (
    <Menu.Root>
      <Menu.Trigger asChild>
        <Button variant="solid" bg="primary" size="sm">
          {t("fields.labels.display")}
        </Button>
      </Menu.Trigger>
      <Portal>
        <Menu.Positioner>
          <Menu.Content>
            <Menu.ItemGroup>
              {items.map(({ title, value }) => (
                <Menu.CheckboxItem
                  key={value}
                  value={value}
                  checked={group.isChecked(value)}
                  onCheckedChange={() => toggleField(value)}
                >
                  {t(title)}
                  <Menu.ItemIndicator />
                </Menu.CheckboxItem>
              ))}
            </Menu.ItemGroup>
          </Menu.Content>
        </Menu.Positioner>
      </Portal>
    </Menu.Root>
  );
}
