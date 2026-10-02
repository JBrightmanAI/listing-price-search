import { useCallback, useState } from 'react'
import type { FieldErrors } from '@/api'
import { hasErrors, validateForm, type FormField, type SearchState } from '@/features/listing-search/model'

/**
 * Draft filter values the user is editing, separate from the applied search the results are for.
 * Nothing is searched until the draft is submitted and passes validation.
 */
export function useSearchForm(applied: SearchState, apply: (next: SearchState) => void) {
  const [draft, setDraft] = useState(applied.form)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [syncedWith, setSyncedWith] = useState(applied)

  // When the applied search changes from outside (back/forward, paging, reset), show its values.
  // Adjusting state during render avoids the extra paint an effect would cause.
  if (syncedWith !== applied) {
    setSyncedWith(applied)
    setDraft(applied.form)
    setErrors({})
  }

  const setField = useCallback((field: FormField, value: string) => {
    setDraft((current) => ({ ...current, [field]: value }))
    // Editing a field clears its stale message; other fields keep theirs.
    setErrors(({ [field]: _cleared, ...rest }) => rest)
  }, [])

  const submit = useCallback(() => {
    const validation = validateForm(draft)
    if (hasErrors(validation)) {
      setErrors(validation)
      return
    }
    // New filters always start from the first page.
    apply({ form: draft, page: 1 })
  }, [draft, apply])

  return { draft, errors, setField, submit }
}
