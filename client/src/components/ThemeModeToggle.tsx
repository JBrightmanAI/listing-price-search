import DarkModeOutlined from '@mui/icons-material/DarkModeOutlined'
import LightModeOutlined from '@mui/icons-material/LightModeOutlined'
import SettingsBrightnessOutlined from '@mui/icons-material/SettingsBrightnessOutlined'
import { useColorScheme } from '@mui/material/styles'
import ToggleButton from '@mui/material/ToggleButton'
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup'
import Tooltip from '@mui/material/Tooltip'

type Mode = 'light' | 'system' | 'dark'

const OPTIONS = [
  { value: 'light', label: 'Light theme', Icon: LightModeOutlined },
  { value: 'system', label: 'Match system theme', Icon: SettingsBrightnessOutlined },
  { value: 'dark', label: 'Dark theme', Icon: DarkModeOutlined },
] as const satisfies readonly { value: Mode; label: string; Icon: unknown }[]

/** Light / System / Dark. "System" follows the OS setting live; the choice persists across visits. */
export function ThemeModeToggle() {
  const { mode, setMode } = useColorScheme()

  // Unknown for the very first render; rendering nothing avoids a toggle that jumps once it is known.
  if (!mode) return null

  return (
    <ToggleButtonGroup
      value={mode}
      exclusive
      size="small"
      aria-label="Colour theme"
      onChange={(_, next: Mode | null) => {
        // Clicking the active option would deselect it; a theme must always be selected.
        if (next) setMode(next)
      }}
    >
      {OPTIONS.map(({ value, label, Icon }) => (
        <Tooltip key={value} title={label}>
          <ToggleButton value={value} aria-label={label}>
            <Icon fontSize="small" />
          </ToggleButton>
        </Tooltip>
      ))}
    </ToggleButtonGroup>
  )
}
