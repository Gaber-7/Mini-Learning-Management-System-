import { TestBed } from '@angular/core/testing';
import { appInterceptor } from './interceptors';  

describe('Interceptors', () => {
  let service: typeof appInterceptor;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(appInterceptor);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
