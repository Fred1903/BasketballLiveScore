import { TestBed } from '@angular/core/testing';

import { MatchSettingsService } from './match-settings.service';

describe('MatchSettingsService', () => {
  let service: MatchSettingsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MatchSettingsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
