import { useTranslation } from "react-i18next"
import { Button } from "../Button/Button"
import { useState, useEffect } from "react"
import { api } from "../../api/fetchClient.js"

import "./Sidebar.css"

export function Sidebar({ boards, workspaces, onWorkspaceSelect }) {
  const { t } = useTranslation()

  return (
    <div className="sidebar">
      <select
        onChange={(e) => {
          const selectedWorkspace = workspaces.find(w => w.id === Number(e.target.value))
          if (selectedWorkspace) {
            onWorkspaceSelect(selectedWorkspace)
          }
        }}
        className="select"
        defaultValue=""
      >
        <option disabled value="">
          {t("workspaceStub")}
        </option>
        {workspaces.map((e) => (
          <option key={e.id} value={e.id}>
            {e.name}
          </option>
        ))}
      </select>
      <Button content={t('createWorkspace')} />
    </div>
  )
}